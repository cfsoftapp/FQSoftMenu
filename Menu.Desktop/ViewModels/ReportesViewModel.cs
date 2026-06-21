using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Menu.DTOs.Reportes;
using Menu.Services.Reportes;

namespace Menu.Desktop.ViewModels;

public sealed class ReportesViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private readonly IReporteService _reporteService;
    private readonly AsyncRelayCommand _buscarCommand;
    private readonly RelayCommand _anteriorCommand;
    private readonly RelayCommand _siguienteCommand;
    private DateTime _fechaInicio = DateTime.Today;
    private DateTime _fechaFin = DateTime.Today;
    private string _search = string.Empty;
    private string _detailSearch = string.Empty;
    private bool _isBusy;
    private int _pageSize = 10;
    private int _currentPage = 1;
    private int _selectedTabIndex;
    private ReporteResumenDto _resumen = new();
    private List<ReporteEmpleadoRowViewModel> _allRows = new();
    private string _estado = "Consulta los consumos y cobros del periodo.";
    private string _estadoBackground = "#EEF6FF";
    private string _estadoBorderBrush = "#90CAF9";
    private string _estadoForeground = "#1565C0";

    public ReportesViewModel(IReporteService reporteService)
    {
        _reporteService = reporteService;
        _buscarCommand = new AsyncRelayCommand(BuscarAsync, () => !IsBusy);
        _anteriorCommand = new RelayCommand(IrAnterior, () => CurrentPage > 1);
        _siguienteCommand = new RelayCommand(IrSiguiente, () => CurrentPage < TotalPages);
    }

    public ObservableCollection<ReporteEmpleadoRowViewModel> Empleados { get; } = new();

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
        set => SetProperty(ref _search, value);
    }

    public string DetailSearch
    {
        get => _detailSearch;
        set
        {
            if (SetProperty(ref _detailSearch, value))
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

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetProperty(ref _selectedTabIndex, value))
                OnPropertyChanged(nameof(IsSummaryTab));
        }
    }

    public bool IsSummaryTab => SelectedTabIndex == 0;

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

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(FilteredRows.Count / (double)PageSize));

    public string PageText => $"Pagina {CurrentPage} de {TotalPages}";

    public string RangeText
    {
        get
        {
            if (FilteredRows.Count == 0)
                return "0 comensales";

            var from = ((CurrentPage - 1) * PageSize) + 1;
            var to = Math.Min(CurrentPage * PageSize, FilteredRows.Count);
            return $"{from}-{to} de {FilteredRows.Count}";
        }
    }

    public int DetailEmployeeCount => FilteredRows.Count;

    public int DetailMenuCount => FilteredRows.Sum(x => x.TotalMenus);

    public int TotalAlmuerzos => FilteredRows.Sum(x => x.TotalAlmuerzos);

    public int TotalCenas => FilteredRows.Sum(x => x.TotalCenas);

    public string DetailTotalEmpresaText => Money(FilteredRows.Sum(x => x.Reporte.TotalEmpresa));

    public string DetailTotalPlanillaText => Money(FilteredRows.Sum(x => x.Reporte.TotalPlanilla));

    public string DetailTotalCobradoText => Money(FilteredRows.Sum(x => x.Reporte.TotalCobrado));

    public string DetailTotalExtrasText => Money(FilteredRows.Sum(x => x.Reporte.TotalExtrasProductos));

    public string DetailTotalPendienteText => Money(FilteredRows.Sum(x => x.Reporte.TotalPendiente));

    public string DetailTotalGeneralText => Money(FilteredRows.Sum(x => x.Reporte.TotalGeneral));

    public string TotalMenusText => _resumen.TotalMenus.ToString("N0", Culture);

    public string TotalEmpresaText => _resumen.TotalEmpresa.ToString("C2", Culture);

    public string TotalPlanillaText => _resumen.TotalPlanilla.ToString("C2", Culture);

    public string TotalProveedorText => _resumen.TotalProveedor.ToString("C2", Culture);

    public string TotalCobradoText => _resumen.TotalCobradoDirecto.ToString("C2", Culture);

    public string TotalPendienteText => _resumen.TotalPendienteCredito.ToString("C2", Culture);

    public string TotalMenuPagoDirectoText => _resumen.TotalMenuPagoDirecto.ToString("C2", Culture);

    public string TotalMenuCreditoPendienteText => _resumen.TotalMenuCreditoPendiente.ToString("C2", Culture);

    public string TotalMenuCreditoPagadoText => _resumen.TotalMenuCreditoPagado.ToString("C2", Culture);

    public string TotalClasificadoText =>
        (_resumen.TotalEmpresa +
         _resumen.TotalPlanilla +
         _resumen.TotalMenuPagoDirecto +
         _resumen.TotalMenuCreditoPendiente +
         _resumen.TotalMenuCreditoPagado).ToString("C2", Culture);

    public string CobradoEfectivoText => _resumen.CobradoEfectivo.ToString("C2", Culture);

    public string CobradoYapeText => _resumen.CobradoYape.ToString("C2", Culture);

    public string CobradoPlinText => _resumen.CobradoPlin.ToString("C2", Culture);

    public string CreditoPagadoText => _resumen.CreditoPagado.ToString("C2", Culture);

    public string PendienteMenuPrincipalText => _resumen.PendienteMenuPrincipal.ToString("C2", Culture);

    public string PendienteMenuExtraText => _resumen.PendienteMenuExtra.ToString("C2", Culture);

    public string PendienteProductoText => _resumen.PendienteProducto.ToString("C2", Culture);

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

    public Task LoadAsync() => BuscarAsync();

    private List<ReporteEmpleadoRowViewModel> FilteredRows
    {
        get
        {
            if (string.IsNullOrWhiteSpace(DetailSearch))
                return _allRows;

            var term = DetailSearch.Trim();
            return _allRows
                .Where(x =>
                    x.Dni.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Trabajador.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private async Task BuscarAsync()
    {
        if (FechaFin < FechaInicio)
        {
            SetEstado("La fecha fin no puede ser menor que la fecha inicio.", false);
            return;
        }

        IsBusy = true;
        try
        {
            var term = Search.Trim();
            var filtro = new ReporteFiltroDto
            {
                FechaDesde = FechaInicio,
                FechaHasta = FechaFin,
                DniTrabajador = term.All(char.IsDigit) ? term : null,
                NombreTrabajador = term.Length > 0 && !term.All(char.IsDigit) ? term : null
            };

            var resumenTask = _reporteService.ObtenerResumenAsync(filtro);
            var empleadosTask = _reporteService.ObtenerDetalleEmpleadosAsync(filtro);
            await Task.WhenAll(resumenTask, empleadosTask);

            _resumen = await resumenTask;
            _allRows = (await empleadosTask)
                .Select(x => new ReporteEmpleadoRowViewModel(x))
                .ToList();

            CurrentPage = 1;
            RefreshPage();
            NotifySummaryChanged();
            SetEstado(
                _allRows.Count == 0
                    ? "No se encontraron consumos en el periodo seleccionado."
                    : $"Reporte actualizado: {_allRows.Count} comensales encontrados.",
                true);
        }
        catch (Exception ex)
        {
            _resumen = new ReporteResumenDto();
            _allRows.Clear();
            RefreshPage();
            NotifySummaryChanged();
            SetEstado($"No se pudo cargar el reporte: {ex.Message}", false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshPage()
    {
        var filtered = FilteredRows;
        var totalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)PageSize));
        if (CurrentPage > totalPages)
            CurrentPage = totalPages;

        Empleados.Clear();
        foreach (var row in filtered.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            Empleados.Add(row);

        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageText));
        OnPropertyChanged(nameof(RangeText));
        NotifyDetailTotalsChanged();
        _anteriorCommand.RaiseCanExecuteChanged();
        _siguienteCommand.RaiseCanExecuteChanged();
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

    private void NotifySummaryChanged()
    {
        OnPropertyChanged(nameof(TotalMenusText));
        OnPropertyChanged(nameof(TotalEmpresaText));
        OnPropertyChanged(nameof(TotalPlanillaText));
        OnPropertyChanged(nameof(TotalProveedorText));
        OnPropertyChanged(nameof(TotalCobradoText));
        OnPropertyChanged(nameof(TotalPendienteText));
        OnPropertyChanged(nameof(TotalMenuPagoDirectoText));
        OnPropertyChanged(nameof(TotalMenuCreditoPendienteText));
        OnPropertyChanged(nameof(TotalMenuCreditoPagadoText));
        OnPropertyChanged(nameof(TotalClasificadoText));
        OnPropertyChanged(nameof(CobradoEfectivoText));
        OnPropertyChanged(nameof(CobradoYapeText));
        OnPropertyChanged(nameof(CobradoPlinText));
        OnPropertyChanged(nameof(CreditoPagadoText));
        OnPropertyChanged(nameof(PendienteMenuPrincipalText));
        OnPropertyChanged(nameof(PendienteMenuExtraText));
        OnPropertyChanged(nameof(PendienteProductoText));
    }

    private void NotifyDetailTotalsChanged()
    {
        OnPropertyChanged(nameof(DetailEmployeeCount));
        OnPropertyChanged(nameof(DetailMenuCount));
        OnPropertyChanged(nameof(TotalAlmuerzos));
        OnPropertyChanged(nameof(TotalCenas));
        OnPropertyChanged(nameof(DetailTotalEmpresaText));
        OnPropertyChanged(nameof(DetailTotalPlanillaText));
        OnPropertyChanged(nameof(DetailTotalCobradoText));
        OnPropertyChanged(nameof(DetailTotalExtrasText));
        OnPropertyChanged(nameof(DetailTotalPendienteText));
        OnPropertyChanged(nameof(DetailTotalGeneralText));
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

    private void SetEstado(string message, bool success)
    {
        Estado = message;
        if (success)
        {
            EstadoBackground = "#E8F5E9";
            EstadoBorderBrush = "#A5D6A7";
            EstadoForeground = "#2E7D32";
            return;
        }

        EstadoBackground = "#FFF8E1";
        EstadoBorderBrush = "#FFCC80";
        EstadoForeground = "#E65100";
    }

    private static string Money(decimal value) =>
        value == 0 ? "-" : value.ToString("C2", Culture);
}
