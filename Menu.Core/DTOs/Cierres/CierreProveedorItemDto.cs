using Menu.Enums;

namespace Menu.DTOs.Cierres;

public class CierreProveedorItemDto
{
    public int ConsumoMenuId { get; set; }

    public int? ConsumoAdicionalId { get; set; }

    public DateTime Fecha { get; set; }

    public int EmpleadoId { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string EmpleadoNombre { get; set; } = string.Empty;

    public TipoServicioMenu TipoServicio { get; set; }

    public TipoPagoMenu TipoPagoMenu { get; set; }

    public TipoAdicional? TipoAdicional { get; set; }

    public string Concepto { get; set; } = string.Empty;

    public decimal Importe { get; set; }

    public bool EsPlanilla => TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla;

    public bool EsAdicionalEmpresa => ConsumoAdicionalId.HasValue;

    public bool EsMenuEmpresa => !EsAdicionalEmpresa && TipoPagoMenu == TipoPagoMenu.Empresa;

    public string ConceptoCierre => !string.IsNullOrWhiteSpace(Concepto)
        ? Concepto
        : TipoServicio.ToString();

    public bool ExcluirDeProveedor { get; set; }

    public string? MotivoExclusion { get; set; }

    public bool IncluidoProveedor => !ExcluirDeProveedor;
}
