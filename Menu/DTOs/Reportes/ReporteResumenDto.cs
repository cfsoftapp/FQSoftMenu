namespace Menu.DTOs.Reportes;

public class ReporteResumenDto
{
    public int TotalMenus { get; set; }

    // Menu principal y adicionales asumidos por la empresa
    public decimal TotalEmpresa { get; set; }

    public decimal TotalPlanilla { get; set; }
    public decimal TotalMenuPagoDirecto { get; set; }
    public decimal TotalMenuCreditoPendiente { get; set; }
    public decimal TotalMenuCreditoPagado { get; set; }

    // Valor operativo empresa + planilla
    public decimal TotalProveedor { get; set; }

    // Cobros directos
    public decimal CobradoEfectivo { get; set; }

    public decimal CobradoYape { get; set; }
    public decimal CobradoPlin { get; set; }
    public decimal CreditoPagado { get; set; }

    public decimal TotalCobradoDirecto =>
        CobradoEfectivo + CobradoYape + CobradoPlin + CreditoPagado;

    // Pendiente crédito comedor
    public decimal PendienteMenuPrincipal { get; set; }

    public decimal PendienteMenuExtra { get; set; }
    public decimal PendienteProducto { get; set; }

    public decimal TotalPendienteCredito =>
        PendienteMenuPrincipal + PendienteMenuExtra + PendienteProducto;
}
