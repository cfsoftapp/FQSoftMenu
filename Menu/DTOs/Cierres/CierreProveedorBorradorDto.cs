namespace Menu.DTOs.Cierres;

public class CierreProveedorBorradorDto
{
    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }

    public bool YaConfirmado { get; set; }

    public int? CierreProveedorId { get; set; }

    public List<CierreProveedorItemDto> Items { get; set; } = new();

    public int TotalMenusActivos => Items.Count(x => x.TipoPagoMenu == Enums.TipoPagoMenu.Empresa);

    public int TotalMenusPlanilla => Items.Count(x => x.TipoPagoMenu == Enums.TipoPagoMenu.DescuentoPlanilla);

    public int TotalMenusPlanillaExcluidos => Items.Count(x => x.ExcluirDeProveedor);

    public decimal TotalPersonalActivo => Items
        .Where(x => x.TipoPagoMenu == Enums.TipoPagoMenu.Empresa)
        .Sum(x => x.Importe);

    public decimal TotalPlanilla => Items
        .Where(x => x.TipoPagoMenu == Enums.TipoPagoMenu.DescuentoPlanilla && !x.ExcluirDeProveedor)
        .Sum(x => x.Importe);

    public decimal TotalExcluidoRevision => Items
        .Where(x => x.ExcluirDeProveedor)
        .Sum(x => x.Importe);

    public decimal TotalLiquidarProveedor => TotalPersonalActivo + TotalPlanilla;
}
