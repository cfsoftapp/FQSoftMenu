using System.Collections.ObjectModel;
using System.Windows.Input;
using Menu.Models;
using Menu.Security;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class ConfiguracionViewModel : ObservableObject
{
    private readonly ConfiguracionMenuService _configuracionService;
    private readonly TipoEmpleadoService _tipoEmpleadoService;
    private readonly EmpresaClienteService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly AuthStateService _authState;
    private bool _isBusy;
    private decimal _precioMenu;
    private string _fechaActualizacion = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _statusIsError;
    private bool _showTipoForm;
    private int _tipoEditingId;
    private string _tipoNombre = string.Empty;
    private string _tipoDescripcion = string.Empty;
    private bool _showEmpresaForm;
    private int _empresaEditingId;
    private string _empresaNombre = string.Empty;
    private string _empresaRazonSocial = string.Empty;
    private string _empresaRuc = string.Empty;
    private bool _showSucursalForm;
    private int _sucursalEditingId;
    private string _sucursalNombre = string.Empty;
    private string _sucursalDireccion = string.Empty;
    private int? _sucursalEmpresaId;

    public ConfiguracionViewModel(
        ConfiguracionMenuService configuracionService,
        TipoEmpleadoService tipoEmpleadoService,
        EmpresaClienteService empresaService,
        SucursalService sucursalService,
        AuthStateService authState)
    {
        _configuracionService = configuracionService;
        _tipoEmpleadoService = tipoEmpleadoService;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _authState = authState;

        RefreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy && CanView);
        SavePriceCommand = new AsyncRelayCommand(SavePriceAsync, () => !IsBusy && CanEdit);
        NewTipoCommand = new RelayCommand(NewTipo, () => CanEdit);
        EditTipoCommand = new RelayCommand<TipoEmpleado>(EditTipo, x => x is not null && CanEdit);
        ToggleTipoCommand = new RelayCommand<TipoEmpleado>(async x => await ToggleTipoAsync(x), x => x is not null && CanEdit);
        SaveTipoCommand = new AsyncRelayCommand(SaveTipoAsync, () => !IsBusy && CanEdit);
        CancelTipoCommand = new RelayCommand(() => ShowTipoForm = false);
        NewEmpresaCommand = new RelayCommand(NewEmpresa, () => CanEdit);
        EditEmpresaCommand = new RelayCommand<EmpresaCliente>(EditEmpresa, x => x is not null && CanEdit);
        ToggleEmpresaCommand = new RelayCommand<EmpresaCliente>(async x => await ToggleEmpresaAsync(x), x => x is not null && CanEdit);
        SaveEmpresaCommand = new AsyncRelayCommand(SaveEmpresaAsync, () => !IsBusy && CanEdit);
        CancelEmpresaCommand = new RelayCommand(() => ShowEmpresaForm = false);
        NewSucursalCommand = new RelayCommand(NewSucursal, () => CanEdit);
        EditSucursalCommand = new RelayCommand<Sucursal>(EditSucursal, x => x is not null && CanEdit);
        ToggleSucursalCommand = new RelayCommand<Sucursal>(async x => await ToggleSucursalAsync(x), x => x is not null && CanEdit);
        SaveSucursalCommand = new AsyncRelayCommand(SaveSucursalAsync, () => !IsBusy && CanEdit);
        CancelSucursalCommand = new RelayCommand(() => ShowSucursalForm = false);
    }

    public ObservableCollection<TipoEmpleado> TiposEmpleado { get; } = new();
    public ObservableCollection<EmpresaCliente> Empresas { get; } = new();
    public ObservableCollection<EmpresaCliente> EmpresasActivas { get; } = new();
    public ObservableCollection<Sucursal> Sucursales { get; } = new();

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand SavePriceCommand { get; }
    public RelayCommand NewTipoCommand { get; }
    public RelayCommand<TipoEmpleado> EditTipoCommand { get; }
    public RelayCommand<TipoEmpleado> ToggleTipoCommand { get; }
    public AsyncRelayCommand SaveTipoCommand { get; }
    public RelayCommand CancelTipoCommand { get; }
    public RelayCommand NewEmpresaCommand { get; }
    public RelayCommand<EmpresaCliente> EditEmpresaCommand { get; }
    public RelayCommand<EmpresaCliente> ToggleEmpresaCommand { get; }
    public AsyncRelayCommand SaveEmpresaCommand { get; }
    public RelayCommand CancelEmpresaCommand { get; }
    public RelayCommand NewSucursalCommand { get; }
    public RelayCommand<Sucursal> EditSucursalCommand { get; }
    public RelayCommand<Sucursal> ToggleSucursalCommand { get; }
    public AsyncRelayCommand SaveSucursalCommand { get; }
    public RelayCommand CancelSucursalCommand { get; }

    public bool CanView => _authState.TienePermiso(Permisos.ConfiguracionVer);
    public bool CanEdit => _authState.TienePermiso(Permisos.ConfiguracionEditar);
    public string StatusColor => StatusIsError ? "#C62828" : "#087F5B";
    public string TipoFormTitle => TipoEditingId == 0 ? "Nuevo tipo de personal" : "Editar tipo de personal";
    public string EmpresaFormTitle => EmpresaEditingId == 0 ? "Nueva empresa cliente" : "Editar empresa cliente";
    public string SucursalFormTitle => SucursalEditingId == 0 ? "Nueva sucursal" : "Editar sucursal";

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            RefreshCommand.RaiseCanExecuteChanged();
            SavePriceCommand.RaiseCanExecuteChanged();
            SaveTipoCommand.RaiseCanExecuteChanged();
            SaveEmpresaCommand.RaiseCanExecuteChanged();
            SaveSucursalCommand.RaiseCanExecuteChanged();
        }
    }

    public decimal PrecioMenu { get => _precioMenu; set => SetProperty(ref _precioMenu, value); }
    public string FechaActualizacion { get => _fechaActualizacion; private set => SetProperty(ref _fechaActualizacion, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public bool StatusIsError
    {
        get => _statusIsError;
        private set
        {
            if (SetProperty(ref _statusIsError, value))
                OnPropertyChanged(nameof(StatusColor));
        }
    }

    public bool ShowTipoForm { get => _showTipoForm; private set => SetProperty(ref _showTipoForm, value); }
    public int TipoEditingId
    {
        get => _tipoEditingId;
        private set
        {
            if (SetProperty(ref _tipoEditingId, value))
                OnPropertyChanged(nameof(TipoFormTitle));
        }
    }
    public string TipoNombre { get => _tipoNombre; set => SetProperty(ref _tipoNombre, value); }
    public string TipoDescripcion { get => _tipoDescripcion; set => SetProperty(ref _tipoDescripcion, value); }

    public bool ShowEmpresaForm { get => _showEmpresaForm; private set => SetProperty(ref _showEmpresaForm, value); }
    public int EmpresaEditingId
    {
        get => _empresaEditingId;
        private set
        {
            if (SetProperty(ref _empresaEditingId, value))
                OnPropertyChanged(nameof(EmpresaFormTitle));
        }
    }
    public string EmpresaNombre { get => _empresaNombre; set => SetProperty(ref _empresaNombre, value); }
    public string EmpresaRazonSocial { get => _empresaRazonSocial; set => SetProperty(ref _empresaRazonSocial, value); }
    public string EmpresaRuc { get => _empresaRuc; set => SetProperty(ref _empresaRuc, value); }

    public bool ShowSucursalForm { get => _showSucursalForm; private set => SetProperty(ref _showSucursalForm, value); }
    public int SucursalEditingId
    {
        get => _sucursalEditingId;
        private set
        {
            if (SetProperty(ref _sucursalEditingId, value))
                OnPropertyChanged(nameof(SucursalFormTitle));
        }
    }
    public string SucursalNombre { get => _sucursalNombre; set => SetProperty(ref _sucursalNombre, value); }
    public string SucursalDireccion { get => _sucursalDireccion; set => SetProperty(ref _sucursalDireccion, value); }
    public int? SucursalEmpresaId { get => _sucursalEmpresaId; set => SetProperty(ref _sucursalEmpresaId, value); }

    public async Task LoadAsync()
    {
        if (!CanView)
            return;

        IsBusy = true;
        SetStatus("Cargando configuracion...", false);

        try
        {
            var config = await _configuracionService.GetActualAsync();
            var tipos = await _tipoEmpleadoService.GetAllAsync();
            var empresas = await _empresaService.GetAllAsync();
            var empresasActivas = await _empresaService.GetActivasAsync();
            var sucursales = await _sucursalService.GetAllAsync();

            PrecioMenu = config.PrecioMenu;
            FechaActualizacion = config.FechaActualizacion.ToString("dd/MM/yyyy HH:mm");
            Replace(TiposEmpleado, tipos);
            Replace(Empresas, empresas);
            Replace(EmpresasActivas, empresasActivas);
            Replace(Sucursales, sucursales);
            SetStatus("Configuracion actualizada.", false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo cargar la configuracion: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SavePriceAsync()
    {
        await ExecuteAsync(
            () => _configuracionService.UpdatePrecioAsync(PrecioMenu),
            async () => await LoadAsync());
    }

    private void NewTipo()
    {
        TipoEditingId = 0;
        TipoNombre = string.Empty;
        TipoDescripcion = string.Empty;
        ShowTipoForm = true;
    }

    private void EditTipo(TipoEmpleado? tipo)
    {
        if (tipo is null) return;
        TipoEditingId = tipo.Id;
        TipoNombre = tipo.Nombre;
        TipoDescripcion = tipo.Descripcion ?? string.Empty;
        ShowTipoForm = true;
    }

    private async Task SaveTipoAsync()
    {
        var activo = TiposEmpleado.FirstOrDefault(x => x.Id == TipoEditingId)?.Activo ?? true;
        var model = new TipoEmpleado { Id = TipoEditingId, Nombre = TipoNombre, Descripcion = TipoDescripcion, Activo = activo };
        await ExecuteAsync(
            () => TipoEditingId == 0 ? _tipoEmpleadoService.CreateAsync(model) : _tipoEmpleadoService.UpdateAsync(model),
            async () => { ShowTipoForm = false; await LoadAsync(); });
    }

    private async Task ToggleTipoAsync(TipoEmpleado? tipo)
    {
        if (tipo is null) return;
        await ExecuteAsync(() => _tipoEmpleadoService.ToggleActivoAsync(tipo.Id), LoadAsync);
    }

    private void NewEmpresa()
    {
        EmpresaEditingId = 0;
        EmpresaNombre = string.Empty;
        EmpresaRazonSocial = string.Empty;
        EmpresaRuc = string.Empty;
        ShowEmpresaForm = true;
    }

    private void EditEmpresa(EmpresaCliente? empresa)
    {
        if (empresa is null) return;
        EmpresaEditingId = empresa.Id;
        EmpresaNombre = empresa.NombreComercial;
        EmpresaRazonSocial = empresa.RazonSocial ?? string.Empty;
        EmpresaRuc = empresa.Ruc ?? string.Empty;
        ShowEmpresaForm = true;
    }

    private async Task SaveEmpresaAsync()
    {
        var activo = Empresas.FirstOrDefault(x => x.Id == EmpresaEditingId)?.Activo ?? true;
        var model = new EmpresaCliente
        {
            Id = EmpresaEditingId,
            NombreComercial = EmpresaNombre,
            RazonSocial = EmpresaRazonSocial,
            Ruc = EmpresaRuc,
            Activo = activo
        };
        await ExecuteAsync(
            () => EmpresaEditingId == 0 ? _empresaService.CreateAsync(model) : _empresaService.UpdateAsync(model),
            async () => { ShowEmpresaForm = false; await LoadAsync(); });
    }

    private async Task ToggleEmpresaAsync(EmpresaCliente? empresa)
    {
        if (empresa is null) return;
        await ExecuteAsync(() => _empresaService.ToggleActivoAsync(empresa.Id), LoadAsync);
    }

    private void NewSucursal()
    {
        SucursalEditingId = 0;
        SucursalNombre = string.Empty;
        SucursalDireccion = string.Empty;
        SucursalEmpresaId = EmpresasActivas.FirstOrDefault()?.Id;
        ShowSucursalForm = true;
    }

    private void EditSucursal(Sucursal? sucursal)
    {
        if (sucursal is null) return;
        SucursalEditingId = sucursal.Id;
        SucursalNombre = sucursal.Nombre;
        SucursalDireccion = sucursal.Direccion ?? string.Empty;
        SucursalEmpresaId = sucursal.EmpresaClienteId;
        ShowSucursalForm = true;
    }

    private async Task SaveSucursalAsync()
    {
        var activo = Sucursales.FirstOrDefault(x => x.Id == SucursalEditingId)?.Activo ?? true;
        var model = new Sucursal
        {
            Id = SucursalEditingId,
            Nombre = SucursalNombre,
            Direccion = SucursalDireccion,
            EmpresaClienteId = SucursalEmpresaId,
            Activo = activo
        };
        await ExecuteAsync(
            () => SucursalEditingId == 0 ? _sucursalService.CreateAsync(model) : _sucursalService.UpdateAsync(model),
            async () => { ShowSucursalForm = false; await LoadAsync(); });
    }

    private async Task ToggleSucursalAsync(Sucursal? sucursal)
    {
        if (sucursal is null) return;
        await ExecuteAsync(() => _sucursalService.ToggleActivoAsync(sucursal.Id), LoadAsync);
    }

    private async Task ExecuteAsync(
        Func<Task<(bool Success, string Message)>> action,
        Func<Task> onSuccess)
    {
        IsBusy = true;
        try
        {
            var result = await action();
            SetStatus(result.Message, !result.Success);
            if (result.Success)
                await onSuccess();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo completar la operacion: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SetStatus(string message, bool error)
    {
        StatusMessage = message;
        StatusIsError = error;
    }

    private static void Replace<T>(ObservableCollection<T> collection, IEnumerable<T> values)
    {
        collection.Clear();
        foreach (var value in values)
            collection.Add(value);
    }
}
