namespace Menu.DTOs.Cierres;

public class CierreDetalleDto
{
    public DateTime Fecha { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Concepto { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal Importe { get; set; }
}
