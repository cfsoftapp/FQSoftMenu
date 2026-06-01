using Menu.Enums;

namespace Menu.DTOs.Cierres;

public class CierreProveedorItemDto
{
    public int ConsumoMenuId { get; set; }

    public DateTime Fecha { get; set; }

    public int EmpleadoId { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string EmpleadoNombre { get; set; } = string.Empty;

    public TipoServicioMenu TipoServicio { get; set; }

    public TipoPagoMenu TipoPagoMenu { get; set; }

    public decimal Importe { get; set; }

    public bool EsPlanilla => TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla;

    public bool ExcluirDeProveedor { get; set; }

    public string? MotivoExclusion { get; set; }

    public bool IncluidoProveedor => !ExcluirDeProveedor;
}
