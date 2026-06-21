using System.Globalization;
using Menu.DTOs.Reportes;

namespace Menu.Desktop.ViewModels;

public sealed class ReporteEmpleadoRowViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private bool _isExpanded;

    public ReporteEmpleadoRowViewModel(ReporteEmpleadoDto reporte)
    {
        Reporte = reporte;
        Detalles = reporte.Detalles
            .Select(x => new ReporteFechaRowViewModel(x))
            .ToList();
        ToggleExpandedCommand = new RelayCommand(() => IsExpanded = !IsExpanded);
    }

    public ReporteEmpleadoDto Reporte { get; }

    public IReadOnlyList<ReporteFechaRowViewModel> Detalles { get; }

    public RelayCommand ToggleExpandedCommand { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
                OnPropertyChanged(nameof(ExpandIcon));
        }
    }

    public string ExpandIcon => IsExpanded ? "⌃" : "⌄";

    public string Dni => Reporte.Dni;

    public string Trabajador => Reporte.Trabajador;

    public string Categoria => Reporte.Categoria;

    public string Estado => Reporte.Estado;

    public int TotalMenus => Reporte.TotalMenus;

    public int TotalAlmuerzos => Reporte.TotalAlmuerzos;

    public int TotalCenas => Reporte.TotalCenas;

    public string TotalEmpresaText => Reporte.TotalEmpresa.ToString("C2", Culture);

    public string TotalPlanillaText => Reporte.TotalPlanilla.ToString("C2", Culture);

    public string TotalCobradoText => Reporte.TotalCobrado.ToString("C2", Culture);

    public string TotalPendienteText => Reporte.TotalPendiente.ToString("C2", Culture);

    public string TotalGeneralText => Reporte.TotalGeneral.ToString("C2", Culture);

    public string TotalExtrasText => Money(Reporte.TotalExtrasProductos);

    public string EstadoBackground =>
        Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase) ? "#00C853" : "#FF9800";

    private static string Money(decimal value) =>
        value == 0 ? "-" : value.ToString("C2", Culture);
}
