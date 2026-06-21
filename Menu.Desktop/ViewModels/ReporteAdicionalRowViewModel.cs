using System.Globalization;
using Menu.DTOs.Reportes;

namespace Menu.Desktop.ViewModels;

public sealed class ReporteAdicionalRowViewModel
{
    private static readonly CultureInfo Culture = new("es-PE");

    public ReporteAdicionalRowViewModel(ReporteAdicionalDetalleDto adicional)
    {
        Adicional = adicional;
    }

    public ReporteAdicionalDetalleDto Adicional { get; }

    public string Tipo => Adicional.Tipo;

    public string Categoria => Adicional.Categoria;

    public string Descripcion => Adicional.Descripcion;

    public string FormaCobro => Adicional.FormaCobro;

    public string Estado => Adicional.EstadoCobro;

    public string ImporteText => Adicional.Importe.ToString("C2", Culture);
}
