namespace Menu.DTOs;

public class ReporteMensualResumenDto
{
    // Menús principales
    public int TotalMenusEmpresa { get; set; }

    public decimal MontoMenusEmpresa { get; set; }

    public int TotalMenusDescuentoPlanilla { get; set; }
    public decimal MontoMenusDescuentoPlanilla { get; set; }

    public int TotalMenusPagoDirecto { get; set; }
    public decimal MontoMenusPagoDirecto { get; set; }

    public int TotalMenusCreditoPendiente { get; set; }
    public decimal MontoMenusCreditoPendiente { get; set; }

    public int TotalMenusCreditoPagado { get; set; }
    public decimal MontoMenusCreditoPagado { get; set; }

    // Adicionales
    public decimal TotalAdicionalesEfectivo { get; set; }

    public decimal TotalAdicionalesYape { get; set; }
    public decimal TotalAdicionalesPlin { get; set; }

    public decimal TotalAdicionalesCreditoPendiente { get; set; }
    public decimal TotalAdicionalesCreditoPagado { get; set; }

    public decimal TotalMenus =>
        MontoMenusEmpresa +
        MontoMenusDescuentoPlanilla +
        MontoMenusPagoDirecto +
        MontoMenusCreditoPendiente +
        MontoMenusCreditoPagado;

    public decimal TotalAdicionales =>
        TotalAdicionalesEfectivo +
        TotalAdicionalesYape +
        TotalAdicionalesPlin +
        TotalAdicionalesCreditoPendiente +
        TotalAdicionalesCreditoPagado;

    public decimal TotalPagarProveedor =>
    MontoMenusEmpresa +
    MontoMenusDescuentoPlanilla;

    public decimal TotalCobradoDirecto =>
        MontoMenusPagoDirecto +
        MontoMenusCreditoPagado +
        TotalAdicionalesEfectivo +
        TotalAdicionalesYape +
        TotalAdicionalesPlin +
        TotalAdicionalesCreditoPagado;

    public decimal TotalPendienteCredito =>
        MontoMenusCreditoPendiente +
        TotalAdicionalesCreditoPendiente;

    public decimal TotalAPlanilla => MontoMenusDescuentoPlanilla;

    public int CantidadMenus =>
    TotalMenusEmpresa +
    TotalMenusDescuentoPlanilla +
    TotalMenusPagoDirecto +
    TotalMenusCreditoPendiente +
    TotalMenusCreditoPagado;

    public int CantidadMenusProveedor => CantidadMenus;

    public int CantidadMenusPlanilla => TotalMenusDescuentoPlanilla;

    public int CantidadMenusPagoDirectoOCreditoPagado =>
        TotalMenusPagoDirecto + TotalMenusCreditoPagado;

    public int CantidadMenusCreditoPendiente => TotalMenusCreditoPendiente;
}