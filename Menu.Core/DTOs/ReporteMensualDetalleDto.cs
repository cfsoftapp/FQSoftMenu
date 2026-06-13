using Menu.Enums;

namespace Menu.DTOs;

public class ReporteMensualDetalleDto
{
    public DateTime Fecha { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string EmpleadoNombre { get; set; } = string.Empty;

    public CategoriaEmpleado CategoriaEmpleado { get; set; }

    public EstadoEmpleado EstadoEmpleado { get; set; }

    public TipoServicioMenu? TipoServicio { get; set; }

    public TipoPagoMenu? TipoPagoMenu { get; set; }

    public FormaPago? FormaPagoDirectoMenu { get; set; }

    public EstadoCobroAdicional? EstadoCobroMenu { get; set; }

    public decimal PrecioMenu { get; set; }

    public string Adicionales { get; set; } = string.Empty;

    public decimal TotalMenuExtra { get; set; }

    public decimal TotalProductos { get; set; }

    public decimal TotalAdicionales => TotalMenuExtra + TotalProductos;

    public decimal TotalConsumo => PrecioMenu + TotalAdicionales;

    public string UsuarioRegistroNombre { get; set; } = string.Empty;
}