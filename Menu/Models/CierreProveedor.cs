using Menu.Enums;

namespace Menu.Models;

public class CierreProveedor
{
    public int Id { get; set; }

    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }

    public EstadoCierreProveedor Estado { get; set; } = EstadoCierreProveedor.Confirmado;

    public int TotalMenusActivos { get; set; }

    public int TotalMenusPlanilla { get; set; }

    public int TotalMenusPlanillaExcluidos { get; set; }

    public decimal TotalPersonalActivo { get; set; }

    public decimal TotalPlanilla { get; set; }

    public decimal TotalExcluidoRevision { get; set; }

    public decimal TotalLiquidarProveedor { get; set; }

    public string? Observacion { get; set; }

    public int UsuarioConfirmacionId { get; set; }

    public string UsuarioConfirmacionNombre { get; set; } = string.Empty;

    public DateTime FechaConfirmacion { get; set; } = DateTime.Now;

    public ICollection<CierreProveedorDetalle> Detalles { get; set; } = new List<CierreProveedorDetalle>();
}
