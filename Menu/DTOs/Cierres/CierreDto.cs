namespace Menu.DTOs.Cierres;

public class CierreDto
{
    public DateTime Fecha { get; set; }

    public int TotalMenus { get; set; }

    public int TotalAlmuerzos { get; set; }

    public int TotalCenas { get; set; }

    public int TotalAdicionales { get; set; }

    public int TotalAnulados { get; set; }

    public decimal TotalEmpresa { get; set; }

    public decimal TotalPlanilla { get; set; }

    public decimal TotalPagoDirecto { get; set; }

    public decimal TotalCreditoPendiente { get; set; }

    public decimal TotalCreditoPagado { get; set; }

    public decimal CobradoEfectivo { get; set; }

    public decimal CobradoYape { get; set; }

    public decimal CobradoPlin { get; set; }

    public decimal TotalProveedor => TotalEmpresa + TotalPlanilla;

    public decimal TotalCobrado => CobradoEfectivo + CobradoYape + CobradoPlin + TotalCreditoPagado;

    public decimal TotalGeneral => TotalProveedor + TotalCobrado + TotalCreditoPendiente;

    public List<CierreDetalleDto> Detalles { get; set; } = new();
}
