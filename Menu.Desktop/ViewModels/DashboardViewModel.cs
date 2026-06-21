using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Menu.DTOs.Reportes;
using Menu.Services.Reportes;

namespace Menu.Desktop.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly IReporteService _reporteService;
    private readonly AsyncRelayCommand _refreshCommand;
    private DateTime _fecha = DateTime.Today;
    private string _estado = "Listo para consultar.";
    private bool _isBusy;

    public DashboardViewModel(IReporteService reporteService)
    {
        _reporteService = reporteService;
        _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
    }

    public DateTime Fecha
    {
        get => _fecha;
        set => SetProperty(ref _fecha, value.Date);
    }

    public string Estado
    {
        get => _estado;
        private set => SetProperty(ref _estado, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                _refreshCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<DashboardMetricViewModel> Metrics { get; } = new();

    public ICommand RefreshCommand => _refreshCommand;

    public async Task LoadAsync()
    {
        IsBusy = true;
        Estado = "Consultando resumen...";

        try
        {
            var resumen = await _reporteService.ObtenerResumenAsync(new ReporteFiltroDto
            {
                FechaDesde = Fecha,
                FechaHasta = Fecha
            });

            Metrics.Clear();
            Metrics.Add(new DashboardMetricViewModel("Menus del dia", resumen.TotalMenus.ToString(CultureInfo.InvariantCulture), "#594AE2"));
            Metrics.Add(new DashboardMetricViewModel("Cargo empresa cliente", FormatMoney(resumen.TotalEmpresa), "#00A676"));
            Metrics.Add(new DashboardMetricViewModel("Descuento planilla", FormatMoney(resumen.TotalPlanilla), "#F57C00"));
            Metrics.Add(new DashboardMetricViewModel("Pendiente comensal", FormatMoney(resumen.TotalPendienteCredito), "#E53935"));
            Metrics.Add(new DashboardMetricViewModel("Cobrado directo", FormatMoney(resumen.TotalCobradoDirecto), "#1976D2"));
            Metrics.Add(new DashboardMetricViewModel("Total a facturar", FormatMoney(resumen.TotalProveedor), "#6D4C41"));

            Estado = $"Resumen actualizado: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            Estado = $"No se pudo cargar el resumen: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string FormatMoney(decimal value)
    {
        return $"S/ {value:N2}";
    }
}
