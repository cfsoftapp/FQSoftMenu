using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Input;
using Menu.DTOs;
using Menu.DTOs.RegistroDiario;
using Menu.Enums;
using Menu.Models;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class RegistroDiarioViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");

    private readonly RegistroDiarioService _registroDiarioService;
    private readonly ConfiguracionMenuService _configuracionMenuService;
    private readonly AuthStateService _authStateService;
    private readonly AsyncRelayCommand _buscarCommand;
    private readonly AsyncRelayCommand _registrarMenuRapidoCommand;
    private readonly AsyncRelayCommand _actualizarRegistrosCommand;
    private readonly AsyncRelayCommand _seleccionarCommand;
    private readonly AsyncRelayCommand _guardarCommand;
    private readonly AsyncRelayCommand _guardarMenusExtraCommand;
    private readonly AsyncRelayCommand _guardarProductosCommand;
    private readonly RelayCommand _nuevoTrabajadorCommand;
    private readonly RelayCommand _agregarMenuExtraCommand;
    private readonly RelayCommand _agregarProductoCommand;
    private readonly RelayCommand<RegistroDiarioAdicionalRowViewModel> _quitarAdicionalCommand;
    private DateTime _fecha = DateTime.Today;
    private string _terminoBusqueda = string.Empty;
    private Empleado? _empleadoSeleccionado;
    private RegistroDiarioEmpleadoResultViewModel? _resultadoSeleccionado;
    private RegistroDiarioAdicionalRowViewModel? _adicionalSeleccionado;
    private bool _registraMenu;
    private bool _registrarMenuAlEscanear = true;
    private TipoServicioMenu _tipoServicio = TipoServicioMenu.Almuerzo;
    private TipoPagoMenu _tipoPagoMenuSuspendido = TipoPagoMenu.DescuentoPlanilla;
    private FormaPago _formaPagoDirectoMenu = FormaPago.Efectivo;
    private decimal _precioMenu;
    private bool _isBusy;
    private string _estado = "Ingresa DNI o nombre para buscar comensal.";
    private string _estadoBackground = "#EEF6FF";
    private string _estadoBorderBrush = "#90CAF9";
    private string _estadoForeground = "#1565C0";

    public RegistroDiarioViewModel(
        RegistroDiarioService registroDiarioService,
        ConfiguracionMenuService configuracionMenuService,
        AuthStateService authStateService)
    {
        _registroDiarioService = registroDiarioService;
        _configuracionMenuService = configuracionMenuService;
        _authStateService = authStateService;
        _buscarCommand = new AsyncRelayCommand(BuscarAsync, CanExecute);
        _registrarMenuRapidoCommand = new AsyncRelayCommand(RegistrarMenuRapidoAsync, CanExecute);
        _actualizarRegistrosCommand = new AsyncRelayCommand(CargarRegistrosRecientesAsync, CanExecute);
        _seleccionarCommand = new AsyncRelayCommand(SeleccionarResultadoAsync, CanSeleccionar);
        _guardarCommand = new AsyncRelayCommand(GuardarAsync, CanGuardar);
        _guardarMenusExtraCommand = new AsyncRelayCommand(
            () => GuardarAdicionalesAsync(TipoAdicional.MenuExtra, "Menus extra"),
            () => CanGuardarTipo(TipoAdicional.MenuExtra));
        _guardarProductosCommand = new AsyncRelayCommand(
            () => GuardarAdicionalesAsync(TipoAdicional.Producto, "Productos"),
            () => CanGuardarTipo(TipoAdicional.Producto));
        _nuevoTrabajadorCommand = new RelayCommand(LimpiarTrabajador);
        _agregarMenuExtraCommand = new RelayCommand(AgregarMenuExtra, CanEditarConsumo);
        _agregarProductoCommand = new RelayCommand(AgregarProducto, CanEditarConsumo);
        _quitarAdicionalCommand = new RelayCommand<RegistroDiarioAdicionalRowViewModel>(QuitarAdicional, CanQuitarAdicional);
        Adicionales.CollectionChanged += OnAdicionalesChanged;
    }

    public ObservableCollection<RegistroDiarioEmpleadoResultViewModel> ResultadosBusqueda { get; } = new();

    public ObservableCollection<RegistroDiarioConsumoRowViewModel> ConsumosDelDia { get; } = new();

    public ObservableCollection<RegistroComensalRecienteDto> RegistrosRecientes { get; } = new();

    public ObservableCollection<RegistroDiarioAdicionalRowViewModel> Adicionales { get; } = new();

    public IEnumerable<RegistroDiarioAdicionalRowViewModel> MenusExtra =>
        Adicionales.Where(x => x.TipoAdicional == TipoAdicional.MenuExtra);

    public IEnumerable<RegistroDiarioAdicionalRowViewModel> ProductosAdicionales =>
        Adicionales.Where(x => x.TipoAdicional == TipoAdicional.Producto);

    public int MenusExtraCount =>
        Adicionales.Count(x => x.TipoAdicional == TipoAdicional.MenuExtra);

    public int ProductosAdicionalesCount =>
        Adicionales.Count(x => x.TipoAdicional == TipoAdicional.Producto);

    public bool HasMenusExtra => MenusExtraCount > 0;

    public bool HasProductosAdicionales => ProductosAdicionalesCount > 0;

    public string MenusExtraTotalTexto =>
        Adicionales
            .Where(x => x.TipoAdicional == TipoAdicional.MenuExtra)
            .Sum(x => x.Precio)
            .ToString("C2", Culture);

    public string ProductosAdicionalesTotalTexto =>
        Adicionales
            .Where(x => x.TipoAdicional == TipoAdicional.Producto)
            .Sum(x => x.Precio)
            .ToString("C2", Culture);

    public IReadOnlyList<OptionViewModel<TipoServicioMenu>> TiposServicio { get; } =
        new[]
        {
            new OptionViewModel<TipoServicioMenu>(TipoServicioMenu.Almuerzo, "Almuerzo"),
            new OptionViewModel<TipoServicioMenu>(TipoServicioMenu.Cena, "Cena")
        };

    public IReadOnlyList<OptionViewModel<TipoPagoMenu>> TiposPagoSuspendido { get; } =
        new[]
        {
            new OptionViewModel<TipoPagoMenu>(TipoPagoMenu.DescuentoPlanilla, "Descuento planilla"),
            new OptionViewModel<TipoPagoMenu>(TipoPagoMenu.PagoDirecto, "Pago directo"),
            new OptionViewModel<TipoPagoMenu>(TipoPagoMenu.CreditoComedor, "Pendiente del comensal")
        };

    public IReadOnlyList<OptionViewModel<FormaPago>> FormasPagoDirecto { get; } =
        new[]
        {
            new OptionViewModel<FormaPago>(FormaPago.Efectivo, "Efectivo"),
            new OptionViewModel<FormaPago>(FormaPago.Yape, "Yape"),
            new OptionViewModel<FormaPago>(FormaPago.Plin, "Plin")
        };

    public IReadOnlyList<OptionViewModel<CategoriaConsumoAdicional>> CategoriasMenuExtra { get; } =
        new[]
        {
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.MenuCarta, "Menu carta"),
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Otro, "Otro")
        };

    public IReadOnlyList<OptionViewModel<CategoriaConsumoAdicional>> CategoriasProducto { get; } =
        new[]
        {
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Bebida, "Bebida"),
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Galleta, "Galleta"),
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Postre, "Postre"),
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Snack, "Snack"),
            new OptionViewModel<CategoriaConsumoAdicional>(CategoriaConsumoAdicional.Otro, "Otro")
        };

    public IReadOnlyList<OptionViewModel<FormaCobroAdicional>> FormasCobroAdicional { get; } =
        new[]
        {
            new OptionViewModel<FormaCobroAdicional>(FormaCobroAdicional.Efectivo, "Efectivo"),
            new OptionViewModel<FormaCobroAdicional>(FormaCobroAdicional.Yape, "Yape"),
            new OptionViewModel<FormaCobroAdicional>(FormaCobroAdicional.Plin, "Plin"),
            new OptionViewModel<FormaCobroAdicional>(FormaCobroAdicional.CreditoComedor, "Pendiente del comensal"),
            new OptionViewModel<FormaCobroAdicional>(FormaCobroAdicional.Empresa, "Empresa cliente")
        };

    public ICommand BuscarCommand => _buscarCommand;

    public ICommand RegistrarMenuRapidoCommand => _registrarMenuRapidoCommand;

    public ICommand ActualizarRegistrosCommand => _actualizarRegistrosCommand;

    public ICommand SeleccionarCommand => _seleccionarCommand;

    public ICommand GuardarCommand => _guardarCommand;

    public ICommand GuardarMenusExtraCommand => _guardarMenusExtraCommand;

    public ICommand GuardarProductosCommand => _guardarProductosCommand;

    public ICommand NuevoTrabajadorCommand => _nuevoTrabajadorCommand;

    public ICommand AgregarMenuExtraCommand => _agregarMenuExtraCommand;

    public ICommand AgregarProductoCommand => _agregarProductoCommand;

    public ICommand QuitarAdicionalCommand => _quitarAdicionalCommand;

    public DateTime Fecha
    {
        get => _fecha;
        set => SetProperty(ref _fecha, value.Date);
    }

    public string TerminoBusqueda
    {
        get => _terminoBusqueda;
        set => SetProperty(ref _terminoBusqueda, value);
    }

    public RegistroDiarioEmpleadoResultViewModel? ResultadoSeleccionado
    {
        get => _resultadoSeleccionado;
        set
        {
            if (SetProperty(ref _resultadoSeleccionado, value))
                _seleccionarCommand.RaiseCanExecuteChanged();
        }
    }

    public RegistroDiarioAdicionalRowViewModel? AdicionalSeleccionado
    {
        get => _adicionalSeleccionado;
        set
        {
            if (SetProperty(ref _adicionalSeleccionado, value))
                _quitarAdicionalCommand.RaiseCanExecuteChanged();
        }
    }

    public bool RegistraMenu
    {
        get => _registraMenu;
        set
        {
            if (SetProperty(ref _registraMenu, value))
            {
                OnPropertyChanged(nameof(PrecioMenuTexto));
                OnPropertyChanged(nameof(TipoPagoMenuTexto));
                NotifyTotalsChanged();
                _guardarCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool RegistrarMenuAlEscanear
    {
        get => _registrarMenuAlEscanear;
        set
        {
            if (SetProperty(ref _registrarMenuAlEscanear, value))
                OnPropertyChanged(nameof(ModoEscaneoTexto));
        }
    }

    public string ModoEscaneoTexto => RegistrarMenuAlEscanear
        ? "Enter registra inmediatamente el menu principal."
        : "Enter selecciona al comensal para registrar adicionales.";

    public TipoServicioMenu TipoServicio
    {
        get => _tipoServicio;
        set => SetProperty(ref _tipoServicio, value);
    }

    public TipoPagoMenu TipoPagoMenuSuspendido
    {
        get => _tipoPagoMenuSuspendido;
        set
        {
            if (SetProperty(ref _tipoPagoMenuSuspendido, value))
            {
                OnPropertyChanged(nameof(IsPagoDirectoVisible));
                OnPropertyChanged(nameof(TipoPagoMenuTexto));
                NotifyTotalsChanged();
            }
        }
    }

    public FormaPago FormaPagoDirectoMenu
    {
        get => _formaPagoDirectoMenu;
        set
        {
            if (SetProperty(ref _formaPagoDirectoMenu, value))
                OnPropertyChanged(nameof(TipoPagoMenuTexto));
        }
    }

    public string EmpleadoTexto => _empleadoSeleccionado is null
        ? "Sin comensal seleccionado"
        : $"{_empleadoSeleccionado.Dni} - {_empleadoSeleccionado.NombreCompleto} ({_empleadoSeleccionado.Estado})";

    public string EmpleadoNombre => _empleadoSeleccionado?.NombreCompleto ?? string.Empty;

    public string EmpleadoDni => _empleadoSeleccionado?.Dni ?? string.Empty;

    public string EmpleadoEmpresaCliente => _empleadoSeleccionado?.EmpresaClienteNombre ?? string.Empty;

    public string EmpleadoSucursal => _empleadoSeleccionado?.SucursalNombre ?? string.Empty;

    public string EmpleadoEstadoTexto => _empleadoSeleccionado is null
        ? string.Empty
        : _empleadoSeleccionado.Estado == EstadoEmpleado.Suspendido
            ? "Suspendido"
            : "Activo";

    public bool IsEmpleadoSelected => _empleadoSeleccionado is not null;

    public bool HasResultadosBusqueda => ResultadosBusqueda.Count > 0 && !IsEmpleadoSelected;

    public bool HasRegistrosRecientes => RegistrosRecientes.Count > 0;

    public bool IsEmpleadoSuspendido => _empleadoSeleccionado?.Estado == EstadoEmpleado.Suspendido;

    public bool IsPagoDirectoVisible => IsEmpleadoSuspendido &&
                                        TipoPagoMenuSuspendido == TipoPagoMenu.PagoDirecto;

    public bool HasAdicionales => Adicionales.Count > 0;

    public string TipoPagoMenuTexto
    {
        get
        {
            if (!RegistraMenu || _empleadoSeleccionado is null)
                return "No aplica";

            if (_empleadoSeleccionado.Estado == EstadoEmpleado.Activo)
                return "Empresa cliente";

            return TipoPagoMenuSuspendido switch
            {
                TipoPagoMenu.DescuentoPlanilla => "Descuento planilla",
                TipoPagoMenu.PagoDirecto => $"Pago directo - {FormaPagoDirectoMenu}",
                TipoPagoMenu.CreditoComedor => "Pendiente del comensal",
                _ => "No aplica"
            };
        }
    }

    public string TotalPagaEmpresaTexto => TotalPagaEmpresa.ToString("C2", Culture);

    public string TotalDescuentoPlanillaTexto => TotalDescuentoPlanilla.ToString("C2", Culture);

    public string TotalCobraTrabajadorTexto => TotalCobraTrabajador.ToString("C2", Culture);

    public string TotalPagadoTexto => TotalPagado.ToString("C2", Culture);

    public string TotalPendienteCreditoTexto => TotalPendienteCredito.ToString("C2", Culture);

    public string PrecioMenuTexto => RegistraMenu
        ? _precioMenu.ToString("C2", Culture)
        : "S/ 0.00";

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _buscarCommand.RaiseCanExecuteChanged();
                _registrarMenuRapidoCommand.RaiseCanExecuteChanged();
                _actualizarRegistrosCommand.RaiseCanExecuteChanged();
                _seleccionarCommand.RaiseCanExecuteChanged();
                _guardarCommand.RaiseCanExecuteChanged();
                _guardarMenusExtraCommand.RaiseCanExecuteChanged();
                _guardarProductosCommand.RaiseCanExecuteChanged();
                _agregarMenuExtraCommand.RaiseCanExecuteChanged();
                _agregarProductoCommand.RaiseCanExecuteChanged();
                _quitarAdicionalCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Estado
    {
        get => _estado;
        private set => SetProperty(ref _estado, value);
    }

    public string EstadoBackground
    {
        get => _estadoBackground;
        private set => SetProperty(ref _estadoBackground, value);
    }

    public string EstadoBorderBrush
    {
        get => _estadoBorderBrush;
        private set => SetProperty(ref _estadoBorderBrush, value);
    }

    public string EstadoForeground
    {
        get => _estadoForeground;
        private set => SetProperty(ref _estadoForeground, value);
    }

    public async Task LoadAsync()
    {
        var config = await _configuracionMenuService.GetActualAsync();
        _precioMenu = config.PrecioMenu;
        await CargarRegistrosRecientesAsync();
        OnPropertyChanged(nameof(PrecioMenuTexto));
        NotifyTotalsChanged();
    }

    private bool CanExecute()
    {
        return !IsBusy;
    }

    private bool CanSeleccionar()
    {
        return !IsBusy && ResultadoSeleccionado is not null;
    }

    private bool CanGuardar()
    {
        return !IsBusy &&
               _empleadoSeleccionado is not null &&
               Adicionales.Count > 0;
    }

    private bool CanGuardarTipo(TipoAdicional tipo)
    {
        return !IsBusy &&
               _empleadoSeleccionado is not null &&
               Adicionales.Any(x => x.TipoAdicional == tipo);
    }

    private bool CanEditarConsumo()
    {
        return !IsBusy && _empleadoSeleccionado is not null;
    }

    private bool CanQuitarAdicional(RegistroDiarioAdicionalRowViewModel? adicional)
    {
        return !IsBusy && adicional is not null;
    }

    private decimal TotalMenu => RegistraMenu ? _precioMenu : 0m;

    private bool MenuPagaEmpresa => _empleadoSeleccionado?.Estado == EstadoEmpleado.Activo && RegistraMenu;

    private bool MenuDescuentoPlanilla =>
        _empleadoSeleccionado?.Estado == EstadoEmpleado.Suspendido &&
        RegistraMenu &&
        TipoPagoMenuSuspendido == TipoPagoMenu.DescuentoPlanilla;

    private bool MenuPagoDirecto =>
        _empleadoSeleccionado?.Estado == EstadoEmpleado.Suspendido &&
        RegistraMenu &&
        TipoPagoMenuSuspendido == TipoPagoMenu.PagoDirecto;

    private bool MenuCreditoComedor =>
        _empleadoSeleccionado?.Estado == EstadoEmpleado.Suspendido &&
        RegistraMenu &&
        TipoPagoMenuSuspendido == TipoPagoMenu.CreditoComedor;

    private decimal TotalPagaEmpresa =>
        (MenuPagaEmpresa ? TotalMenu : 0m) +
        Adicionales.Where(x => x.FormaCobro == FormaCobroAdicional.Empresa).Sum(x => x.Precio);

    private decimal TotalDescuentoPlanilla => MenuDescuentoPlanilla ? TotalMenu : 0m;

    private decimal TotalCobraTrabajador =>
        (MenuDescuentoPlanilla || MenuPagoDirecto || MenuCreditoComedor ? TotalMenu : 0m) +
        Adicionales.Where(x => x.FormaCobro != FormaCobroAdicional.Empresa).Sum(x => x.Precio);

    private decimal TotalPagado =>
        (MenuPagoDirecto ? TotalMenu : 0m) +
        Adicionales
            .Where(x => x.FormaCobro is FormaCobroAdicional.Efectivo or FormaCobroAdicional.Yape or FormaCobroAdicional.Plin)
            .Sum(x => x.Precio);

    private decimal TotalPendienteCredito =>
        (MenuCreditoComedor ? TotalMenu : 0m) +
        Adicionales.Where(x => x.FormaCobro == FormaCobroAdicional.CreditoComedor).Sum(x => x.Precio);

    private async Task BuscarAsync()
    {
        if (string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            SetEstado("Ingrese DNI o nombre del comensal.", EstadoVisual.Warning);
            return;
        }

        IsBusy = true;

        try
        {
            ResultadosBusqueda.Clear();
            ResultadoSeleccionado = null;
            _empleadoSeleccionado = null;
            NotifyEmpleadoStateChanged();

            var resultados = await _registroDiarioService.BuscarEmpleadosActivosAsync(TerminoBusqueda);

            foreach (var empleado in resultados)
                ResultadosBusqueda.Add(new RegistroDiarioEmpleadoResultViewModel(empleado));

            OnPropertyChanged(nameof(HasResultadosBusqueda));

            if (ResultadosBusqueda.Count == 0)
            {
                SetEstado("No se encontro un comensal activo con ese dato.", EstadoVisual.Warning);
                return;
            }

            var exacto = ResultadosBusqueda
                .FirstOrDefault(x => string.Equals(x.Dni, TerminoBusqueda.Trim(), StringComparison.OrdinalIgnoreCase));

            if (exacto is not null || ResultadosBusqueda.Count == 1)
            {
                ResultadoSeleccionado = exacto ?? ResultadosBusqueda[0];
                await SeleccionarResultadoAsync();
                return;
            }

            SetEstado("Hay varias coincidencias. Seleccione un comensal.", EstadoVisual.Info);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegistrarMenuRapidoAsync()
    {
        if (string.IsNullOrWhiteSpace(TerminoBusqueda))
        {
            SetEstado("Ingrese DNI para registrar menu rapido.", EstadoVisual.Warning);
            return;
        }

        if (!_authStateService.EstaAutenticado || _authStateService.UsuarioActual is null)
        {
            SetEstado("Debe iniciar sesion para registrar consumos.", EstadoVisual.Warning);
            return;
        }

        IsBusy = true;

        try
        {
            var resultados = await _registroDiarioService.BuscarEmpleadosActivosAsync(TerminoBusqueda);
            var empleado = resultados
                .FirstOrDefault(x => string.Equals(x.Dni, TerminoBusqueda.Trim(), StringComparison.OrdinalIgnoreCase));

            if (empleado is null)
            {
                if (resultados.Count == 1)
                {
                    empleado = resultados[0];
                }
                else if (resultados.Count > 1)
                {
                    ResultadosBusqueda.Clear();

                    foreach (var resultado in resultados)
                        ResultadosBusqueda.Add(new RegistroDiarioEmpleadoResultViewModel(resultado));

                    OnPropertyChanged(nameof(HasResultadosBusqueda));
                    SetEstado("Hay varias coincidencias. Use Buscar y seleccione el comensal.", EstadoVisual.Info);
                    return;
                }
            }

            if (empleado is null)
            {
                LimpiarTrabajador();
                SetEstado("No se encontro un comensal activo con ese DNI.", EstadoVisual.Warning);
                return;
            }

            var input = new RegistroDiarioInputDto
            {
                EmpleadoId = empleado.Id,
                Fecha = Fecha,
                TipoServicio = TipoServicio,
                RegistraMenu = true,
                TipoPagoMenuSuspendido = empleado.Estado == EstadoEmpleado.Suspendido
                    ? TipoPagoMenuSuspendido
                    : null,
                FormaPagoDirectoMenu = empleado.Estado == EstadoEmpleado.Suspendido &&
                                       TipoPagoMenuSuspendido == TipoPagoMenu.PagoDirecto
                    ? FormaPagoDirectoMenu
                    : null,
                UsuarioRegistroId = _authStateService.UsuarioActual.Id,
                UsuarioRegistroNombre = _authStateService.UsuarioActual.NombreCompleto
            };

            var result = await _registroDiarioService.RegistrarAsync(input);
            if (result.Success)
                await CargarRegistrosRecientesAsync();

            LimpiarTrabajador();
            SetEstado($"{empleado.NombreCompleto}: {result.Message}", result.Success ? EstadoVisual.Success : EstadoVisual.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SeleccionarResultadoAsync()
    {
        if (ResultadoSeleccionado is null)
            return;

        _empleadoSeleccionado = ResultadoSeleccionado.Empleado;
        TerminoBusqueda = _empleadoSeleccionado.Dni;
        RegistraMenu = false;
        ResultadosBusqueda.Clear();
        ResultadoSeleccionado = null;
        NotifyEmpleadoStateChanged();
        _guardarCommand.RaiseCanExecuteChanged();

        await CargarConsumosDelDiaAsync();
        SetEstado($"Comensal seleccionado: {_empleadoSeleccionado.NombreCompleto}.", EstadoVisual.Success);
    }

    private async Task GuardarAsync()
    {
        if (_empleadoSeleccionado is null)
        {
            SetEstado("Primero busque un comensal.", EstadoVisual.Warning);
            return;
        }

        if (!_authStateService.EstaAutenticado || _authStateService.UsuarioActual is null)
        {
            SetEstado("Debe iniciar sesion para registrar consumos.", EstadoVisual.Warning);
            return;
        }

        foreach (var adicional in Adicionales)
        {
            if (string.IsNullOrWhiteSpace(adicional.Descripcion))
            {
                SetEstado("Todos los adicionales deben tener descripcion.", EstadoVisual.Warning);
                return;
            }

            if (adicional.Precio <= 0)
            {
                SetEstado("Todos los adicionales deben tener precio mayor a cero.", EstadoVisual.Warning);
                return;
            }
        }

        IsBusy = true;

        try
        {
            var input = new RegistroDiarioInputDto
            {
                EmpleadoId = _empleadoSeleccionado.Id,
                Fecha = Fecha,
                TipoServicio = TipoServicio,
                RegistraMenu = false,
                TipoPagoMenuSuspendido = null,
                FormaPagoDirectoMenu = null,
                Adicionales = Adicionales.Select(x => x.ToInput()).ToList(),
                UsuarioRegistroId = _authStateService.UsuarioActual.Id,
                UsuarioRegistroNombre = _authStateService.UsuarioActual.NombreCompleto
            };

            var result = await _registroDiarioService.RegistrarAsync(input);
            SetEstado(result.Message, result.Success ? EstadoVisual.Success : EstadoVisual.Error);

            if (result.Success)
            {
                AdicionalSeleccionado = null;
                RegistraMenu = false;
                Adicionales.Clear();
                NotifyAdicionalesChanged();
                await CargarConsumosDelDiaAsync();
                await CargarRegistrosRecientesAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GuardarAdicionalesAsync(TipoAdicional tipo, string etiqueta)
    {
        if (_empleadoSeleccionado is null)
        {
            SetEstado("Primero busque un comensal.", EstadoVisual.Warning);
            return;
        }

        if (!_authStateService.EstaAutenticado || _authStateService.UsuarioActual is null)
        {
            SetEstado("Debe iniciar sesion para registrar consumos.", EstadoVisual.Warning);
            return;
        }

        var items = Adicionales.Where(x => x.TipoAdicional == tipo).ToList();

        if (items.Any(x => string.IsNullOrWhiteSpace(x.Descripcion)))
        {
            SetEstado($"Complete la descripcion de todos los elementos de {etiqueta}.", EstadoVisual.Warning);
            return;
        }

        if (items.Any(x => x.Precio <= 0))
        {
            SetEstado($"Todos los elementos de {etiqueta} deben tener precio mayor a cero.", EstadoVisual.Warning);
            return;
        }

        IsBusy = true;

        try
        {
            var input = new RegistroDiarioInputDto
            {
                EmpleadoId = _empleadoSeleccionado.Id,
                Fecha = Fecha,
                TipoServicio = TipoServicio,
                RegistraMenu = false,
                Adicionales = items.Select(x => x.ToInput()).ToList(),
                UsuarioRegistroId = _authStateService.UsuarioActual.Id,
                UsuarioRegistroNombre = _authStateService.UsuarioActual.NombreCompleto
            };

            var result = await _registroDiarioService.RegistrarAsync(input);
            SetEstado(
                $"{etiqueta}: {result.Message}",
                result.Success ? EstadoVisual.Success : EstadoVisual.Error);

            if (!result.Success)
                return;

            foreach (var item in items)
                Adicionales.Remove(item);

            AdicionalSeleccionado = null;
            NotifyAdicionalesChanged();
            await CargarConsumosDelDiaAsync();
            await CargarRegistrosRecientesAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CargarConsumosDelDiaAsync()
    {
        ConsumosDelDia.Clear();

        if (_empleadoSeleccionado is null)
            return;

        var consumos = await _registroDiarioService
            .GetConsumosDelDiaPorEmpleadoAsync(_empleadoSeleccionado.Id, Fecha);

        foreach (var consumo in consumos)
            ConsumosDelDia.Add(new RegistroDiarioConsumoRowViewModel(consumo));

        OnPropertyChanged(nameof(ConsumosDelDia));
    }

    private async Task CargarRegistrosRecientesAsync()
    {
        RegistrosRecientes.Clear();

        var registros = await _registroDiarioService
            .GetRegistrosMenuRecientesAsync(Fecha);

        foreach (var registro in registros)
            RegistrosRecientes.Add(registro);

        OnPropertyChanged(nameof(RegistrosRecientes));
        OnPropertyChanged(nameof(HasRegistrosRecientes));
    }

    private void LimpiarTrabajador()
    {
        TerminoBusqueda = string.Empty;
        ResultadoSeleccionado = null;
        _empleadoSeleccionado = null;
        ResultadosBusqueda.Clear();
        ConsumosDelDia.Clear();
        AdicionalSeleccionado = null;
        Adicionales.Clear();
        RegistraMenu = false;
        SetEstado("Ingresa DNI o nombre para buscar comensal.", EstadoVisual.Info);
        OnPropertyChanged(nameof(HasAdicionales));
        NotifyTotalsChanged();
        NotifyEmpleadoStateChanged();
        _guardarCommand.RaiseCanExecuteChanged();
    }

    private void AgregarMenuExtra()
    {
        Adicionales.Add(new RegistroDiarioAdicionalRowViewModel(
            TipoAdicional.MenuExtra,
            CategoriaConsumoAdicional.MenuCarta,
            "Menu extra"));
        NotifyAdicionalesChanged();
    }

    private void AgregarProducto()
    {
        Adicionales.Add(new RegistroDiarioAdicionalRowViewModel(
            TipoAdicional.Producto,
            CategoriaConsumoAdicional.Bebida,
            string.Empty));
        NotifyAdicionalesChanged();
    }

    private void QuitarAdicional(RegistroDiarioAdicionalRowViewModel? adicional)
    {
        if (adicional is null)
            return;

        Adicionales.Remove(adicional);
        AdicionalSeleccionado = null;
        NotifyAdicionalesChanged();
    }

    public void DescartarBorradoresVacios(TipoAdicional tipo)
    {
        var borradores = Adicionales
            .Where(x => x.TipoAdicional == tipo &&
                        x.Precio <= 0 &&
                        (string.IsNullOrWhiteSpace(x.Descripcion) ||
                         x.Descripcion == "Menu extra"))
            .ToList();

        if (borradores.Count == 0)
            return;

        foreach (var borrador in borradores)
            Adicionales.Remove(borrador);

        NotifyAdicionalesChanged();
        SetEstado("Se descarto un adicional vacio al cambiar de pestana.", EstadoVisual.Info);
    }

    public async Task AnularConsumoAsync(RegistroDiarioConsumoRowViewModel? consumo, string motivo)
    {
        if (consumo is null)
            return;

        if (!consumo.PuedeAnular)
            return;

        if (string.IsNullOrWhiteSpace(motivo))
        {
            SetEstado("Debe indicar el motivo de anulación.", EstadoVisual.Warning);
            return;
        }

        if (!_authStateService.EstaAutenticado || _authStateService.UsuarioActual is null)
        {
            SetEstado("Debe iniciar sesion para anular consumos.", EstadoVisual.Warning);
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _registroDiarioService.AnularConsumoDiaAsync(
                consumo.Origen,
                consumo.Id,
                _authStateService.UsuarioActual.Id,
                _authStateService.UsuarioActual.NombreCompleto,
                motivo.Trim());

            SetEstado(result.Message, result.Success ? EstadoVisual.Success : EstadoVisual.Error);

            if (result.Success)
                await CargarConsumosDelDiaAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyAdicionalesChanged()
    {
        OnPropertyChanged(nameof(HasAdicionales));
        OnPropertyChanged(nameof(MenusExtra));
        OnPropertyChanged(nameof(ProductosAdicionales));
        OnPropertyChanged(nameof(MenusExtraCount));
        OnPropertyChanged(nameof(ProductosAdicionalesCount));
        OnPropertyChanged(nameof(HasMenusExtra));
        OnPropertyChanged(nameof(HasProductosAdicionales));
        OnPropertyChanged(nameof(MenusExtraTotalTexto));
        OnPropertyChanged(nameof(ProductosAdicionalesTotalTexto));
        NotifyTotalsChanged();
        _guardarCommand.RaiseCanExecuteChanged();
        _guardarMenusExtraCommand.RaiseCanExecuteChanged();
        _guardarProductosCommand.RaiseCanExecuteChanged();
        _quitarAdicionalCommand.RaiseCanExecuteChanged();
    }

    private void OnAdicionalesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (RegistroDiarioAdicionalRowViewModel item in e.NewItems)
                item.PropertyChanged += OnAdicionalChanged;
        }

        if (e.OldItems is not null)
        {
            foreach (RegistroDiarioAdicionalRowViewModel item in e.OldItems)
                item.PropertyChanged -= OnAdicionalChanged;
        }
    }

    private void OnAdicionalChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(RegistroDiarioAdicionalRowViewModel.Precio) or
            nameof(RegistroDiarioAdicionalRowViewModel.FormaCobro))
        {
            OnPropertyChanged(nameof(MenusExtraTotalTexto));
            OnPropertyChanged(nameof(ProductosAdicionalesTotalTexto));
            NotifyTotalsChanged();
        }
    }

    private void NotifyTotalsChanged()
    {
        OnPropertyChanged(nameof(TotalPagaEmpresaTexto));
        OnPropertyChanged(nameof(TotalDescuentoPlanillaTexto));
        OnPropertyChanged(nameof(TotalCobraTrabajadorTexto));
        OnPropertyChanged(nameof(TotalPagadoTexto));
        OnPropertyChanged(nameof(TotalPendienteCreditoTexto));
    }

    private void NotifyEmpleadoStateChanged()
    {
        OnPropertyChanged(nameof(EmpleadoTexto));
        OnPropertyChanged(nameof(EmpleadoNombre));
        OnPropertyChanged(nameof(EmpleadoDni));
        OnPropertyChanged(nameof(EmpleadoEmpresaCliente));
        OnPropertyChanged(nameof(EmpleadoSucursal));
        OnPropertyChanged(nameof(EmpleadoEstadoTexto));
        OnPropertyChanged(nameof(IsEmpleadoSelected));
        OnPropertyChanged(nameof(HasResultadosBusqueda));
        OnPropertyChanged(nameof(IsEmpleadoSuspendido));
        OnPropertyChanged(nameof(IsPagoDirectoVisible));
        OnPropertyChanged(nameof(TipoPagoMenuTexto));
        NotifyTotalsChanged();
        _agregarMenuExtraCommand.RaiseCanExecuteChanged();
        _agregarProductoCommand.RaiseCanExecuteChanged();
    }

    private void SetEstado(string mensaje, EstadoVisual visual)
    {
        Estado = mensaje;

        (EstadoBackground, EstadoBorderBrush, EstadoForeground) = visual switch
        {
            EstadoVisual.Success => ("#E8F5E9", "#81C784", "#2E7D32"),
            EstadoVisual.Warning => ("#FFF8E1", "#FFB74D", "#EF6C00"),
            EstadoVisual.Error => ("#FFEBEE", "#EF9A9A", "#C62828"),
            _ => ("#EEF6FF", "#90CAF9", "#1565C0")
        };
    }

    private enum EstadoVisual
    {
        Info,
        Success,
        Warning,
        Error
    }
}
