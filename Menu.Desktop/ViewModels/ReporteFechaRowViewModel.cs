using System.Globalization;
using Menu.DTOs.Reportes;

namespace Menu.Desktop.ViewModels;

public sealed class ReporteFechaRowViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private bool _isExpanded;

    public ReporteFechaRowViewModel(ReporteEmpleadoFechaDto detalle)
    {
        Detalle = detalle;
        Adicionales = detalle.Adicionales
            .Select(x => new ReporteAdicionalRowViewModel(x))
            .ToList();
        ToggleExpandedCommand = new RelayCommand(
            () => IsExpanded = !IsExpanded,
            () => HasAdicionales);
    }

    public ReporteEmpleadoFechaDto Detalle { get; }

    public IReadOnlyList<ReporteAdicionalRowViewModel> Adicionales { get; }

    public RelayCommand ToggleExpandedCommand { get; }

    public bool HasAdicionales => Adicionales.Count > 0;

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
                OnPropertyChanged(nameof(ExpandIcon));
        }
    }

    public string ExpandIcon => IsExpanded ? "-" : "+";

    public string TotalAdicionalesText =>
        Detalle.Adicionales.Sum(x => x.Importe).ToString("C2", Culture);

    public string Fecha => Detalle.FechaConsumo.ToString("dd/MM/yyyy");

    public string Menu => Detalle.ConsumioMenuPrincipal ? "Si" : "No";

    public string MenuBackground => Detalle.ConsumioMenuPrincipal ? "#00C853" : "#9E9E9E";

    public string TipoPago => EmptyAsDash(Detalle.TipoPagoMenuPrincipal);

    public string MedioPago => EmptyAsDash(Detalle.MedioPago);

    public string EmpresaText => Money(Detalle.ImporteEmpresa);

    public string PlanillaText => Money(Detalle.ImportePlanilla);

    public string CobradoText => Money(Detalle.TotalCobrado);

    public string PendienteText => Money(Detalle.TotalPendiente);

    public string MenuExtraText => Money(Detalle.ImporteMenuExtra);

    public string ProductosText => Money(Detalle.ImporteProductos);

    private static string Money(decimal value) =>
        value == 0 ? "-" : value.ToString("C2", Culture);

    private static string EmptyAsDash(string value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;
}
