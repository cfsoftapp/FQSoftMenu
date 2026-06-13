namespace Menu.DTOs.Reportes;

public class ReporteAdicionalDetalleDto
{
    public string Tipo { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string FormaCobro { get; set; } = string.Empty;
    public string EstadoCobro { get; set; } = string.Empty;
    public decimal Importe { get; set; }
}