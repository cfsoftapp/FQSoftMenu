using Menu.Enums;

namespace Menu.Models;

public class CierreProveedorDetalle
{
    public int Id { get; set; }

    public int CierreProveedorId { get; set; }

    public CierreProveedor CierreProveedor { get; set; } = null!;

    public int ConsumoMenuId { get; set; }

    public DateTime Fecha { get; set; }

    public int EmpleadoId { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string EmpleadoNombre { get; set; } = string.Empty;

    public TipoServicioMenu TipoServicio { get; set; }

    public TipoPagoMenu TipoPagoMenu { get; set; }

    public decimal Importe { get; set; }

    public bool IncluidoProveedor { get; set; }

    public bool ExcluidoPorPagoDirecto { get; set; }

    public string? MotivoExclusion { get; set; }
}
