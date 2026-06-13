using Menu.Enums;

namespace Menu.DTOs.Cierres;

public class CierreProveedorListadoDto
{
    public int Id { get; set; }

    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }

    public EstadoCierreProveedor Estado { get; set; }

    public int TotalMenus { get; set; }

    public decimal TotalLiquidarProveedor { get; set; }

    public decimal TotalExcluidoRevision { get; set; }

    public DateTime FechaRegistro { get; set; }

    public string UsuarioRegistroNombre { get; set; } = string.Empty;
}
