using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Menu.DTOs;
using Menu.Enums;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class CuentasPorCobrarViewModel : ObservableObject
{
    private enum EstadoVisual
    {
        Info,
        Success,
        Warning,
        Error
    }

    private static readonly CultureInfo Culture = new("es-PE");
    private readonly CuentaPorCobrarService _cuentaPorCobrarService;
    private readonly AuthStateService _authStateService;
    private readonly AsyncRelayCommand _buscarCommand;
    private readonly RelayCommand _anteriorCommand;
    private readonly RelayCommand _siguienteCommand;
    private DateTime _fechaInicio = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _fechaFin = DateTime.Today;
    private string _search = string.Empty;
    private bool _isBusy;
    private int _pageSize = 10;
    private int _currentPage = 1;
    private string _estado = "Consulta los consumos pendientes del comensal.";
    private string _estadoBackground = "#EEF6FF";
    private string _estadoBorderBrush = "#90CAF9";
    private string _estadoForeground = "#1565C0";
    private List<CuentaPorCobrarRowViewModel> _allRows = new();

    public CuentasPorCobrarViewModel(
        CuentaPorCobrarService cuentaPorCobrarService,
        AuthStateService authStateService)
    {
        _cuentaPorCobrarService = cuentaPorCobrarService;
        _authStateService = authStateService;
        _buscarCommand = new AsyncRelayCommand(BuscarAsync, () => !IsBusy);
        _anteriorCommand = new RelayCommand(IrAnterior, () => CurrentPage > 1);
        _siguienteCommand = new RelayCommand(IrSiguiente, () => CurrentPage < TotalPages);
    }

    public ObservableCollection<CuentaPorCobrarRowViewModel> Pendientes { get; } = new();

    public IReadOnlyList<int> PageSizes { get; } = new[] { 5, 10, 20, 50 };

    public ICommand BuscarCommand => _buscarCommand;

    public ICommand AnteriorCommand => _anteriorCommand;

    public ICommand SiguienteCommand => _siguienteCommand;

    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetProperty(ref _fechaInicio, value.Date);
    }

    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetProperty(ref _fechaFin, value.Date);
    }

    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
            {
                CurrentPage = 1;
                RefreshPage();
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value))
            {
                CurrentPage = 1;
                RefreshPage();
            }
        }
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(PageText));
                _anteriorCommand.RaiseCanExecuteChanged();
                _siguienteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredRows.Count() / (double)PageSize));

    public string PageText => $"Pagina {CurrentPage} de {TotalPages}";

    public string RangeText
    {
        get
        {
            var count = FilteredRows.Count();
            if (count == 0)
                return "0 pendientes";

            var from = ((CurrentPage - 1) * PageSize) + 1;
            var to = Math.Min(CurrentPage * PageSize, count);
            return $"{from}-{to} de {count}";
        }
    }

    public decimal TotalPendiente => FilteredRows.Sum(x => x.Precio);

    public decimal TotalSeleccionado => SelectedRows.Sum(x => x.Precio);

    public int TotalComensales => FilteredRows.Select(x => x.EmpleadoId).Distinct().Count();

    public int TotalRegistros => FilteredRows.Count();

    public string TotalPendienteText => TotalPendiente.ToString("C2", Culture);

    public string TotalSeleccionadoText => TotalSeleccionado.ToString("C2", Culture);

    public bool CanRegisterPayment => SelectedRows.Count > 0;

    public string SelectedEmployeeName => SelectedRows.FirstOrDefault()?.EmpleadoNombre ?? string.Empty;

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

    private IEnumerable<CuentaPorCobrarRowViewModel> FilteredRows
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Search))
                return _allRows;

            var term = Search.Trim();
            return _allRows.Where(x =>
                x.Dni.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.EmpleadoNombre.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }

    private List<CuentaPorCobrarRowViewModel> SelectedRows =>
        _allRows.Where(x => x.IsSelected).ToList();

    public async Task LoadAsync()
    {
        await BuscarAsync();
    }

    public async Task<ResultadoOperacionDto> RegistrarPagoAsync(
        FormaPagoCredito formaPago,
        DateTime fechaPago,
        string? observacion)
    {
        if (!_authStateService.EstaAutenticado || _authStateService.UsuarioActual is null)
            return ResultadoOperacionDto.Fail("Debe iniciar sesion para registrar el pago.");

        var selected = SelectedRows;
        if (selected.Count == 0)
            return ResultadoOperacionDto.Fail("Debe seleccionar al menos un consumo pendiente.");

        var input = new PagoCreditoInputDto
        {
            EmpleadoId = selected[0].EmpleadoId,
            ConsumoMenuIds = selected
                .Where(x => x.Cuenta.TipoCuenta == TipoCuentaPorCobrar.MenuPrincipal)
                .Select(x => x.Cuenta.ConsumoMenuId)
                .ToList(),
            ConsumoAdicionalIds = selected
                .Where(x => x.Cuenta.TipoCuenta == TipoCuentaPorCobrar.Adicional)
                .Select(x => x.Cuenta.ConsumoAdicionalId)
                .ToList(),
            FormaPago = formaPago,
            FechaPago = fechaPago,
            Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim(),
            UsuarioRegistroId = _authStateService.UsuarioActual.Id,
            UsuarioRegistroNombre = _authStateService.UsuarioActual.NombreCompleto
        };

        IsBusy = true;
        try
        {
            var result = await _cuentaPorCobrarService.RegistrarPagoAsync(input);
            SetEstado(result.Message, result.Success ? EstadoVisual.Success : EstadoVisual.Error);

            if (result.Success)
                await BuscarAsync();

            return result;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task BuscarAsync()
    {
        if (FechaFin < FechaInicio)
        {
            SetEstado("La fecha fin no puede ser menor que la fecha inicio.", EstadoVisual.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var pendientes = await _cuentaPorCobrarService
                .GetPendientesAsync(FechaInicio, FechaFin);

            _allRows = pendientes
                .Select(x => new CuentaPorCobrarRowViewModel(x, ChangeSelection, NotifySelectionChanged))
                .ToList();

            CurrentPage = 1;
            RefreshPage();
            SetEstado(
                pendientes.Count == 0
                    ? "No hay consumos pendientes en el rango seleccionado."
                    : $"Se encontraron {pendientes.Count} consumos pendientes.",
                pendientes.Count == 0 ? EstadoVisual.Info : EstadoVisual.Success);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ChangeSelection(CuentaPorCobrarRowViewModel row, bool selected)
    {
        if (selected)
        {
            var employeeId = SelectedRows.FirstOrDefault()?.EmpleadoId;
            if (employeeId.HasValue && employeeId.Value != row.EmpleadoId)
            {
                SetEstado("Solo puede seleccionar consumos de un mismo comensal.", EstadoVisual.Warning);
                return false;
            }
        }

        return true;
    }

    private void RefreshPage()
    {
        var filtered = FilteredRows.ToList();
        var totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        if (CurrentPage > totalPages)
            CurrentPage = totalPages;

        Pendientes.Clear();
        foreach (var row in filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            Pendientes.Add(row);

        NotifySummaryChanged();
    }

    private void IrAnterior()
    {
        if (CurrentPage <= 1)
            return;

        CurrentPage--;
        RefreshPage();
    }

    private void IrSiguiente()
    {
        if (CurrentPage >= TotalPages)
            return;

        CurrentPage++;
        RefreshPage();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(TotalSeleccionado));
        OnPropertyChanged(nameof(TotalSeleccionadoText));
        OnPropertyChanged(nameof(CanRegisterPayment));
        OnPropertyChanged(nameof(SelectedEmployeeName));
    }

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageText));
        OnPropertyChanged(nameof(RangeText));
        OnPropertyChanged(nameof(TotalPendiente));
        OnPropertyChanged(nameof(TotalPendienteText));
        OnPropertyChanged(nameof(TotalComensales));
        OnPropertyChanged(nameof(TotalRegistros));
        NotifySelectionChanged();
        _anteriorCommand.RaiseCanExecuteChanged();
        _siguienteCommand.RaiseCanExecuteChanged();
    }

    private bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                _buscarCommand.RaiseCanExecuteChanged();
        }
    }

    private void SetEstado(string message, EstadoVisual visual)
    {
        Estado = message;

        (EstadoBackground, EstadoBorderBrush, EstadoForeground) = visual switch
        {
            EstadoVisual.Success => ("#E8F5E9", "#A5D6A7", "#2E7D32"),
            EstadoVisual.Warning => ("#FFF8E1", "#FFCC80", "#E65100"),
            EstadoVisual.Error => ("#FFEBEE", "#EF9A9A", "#C62828"),
            _ => ("#EEF6FF", "#90CAF9", "#1565C0")
        };
    }
}
