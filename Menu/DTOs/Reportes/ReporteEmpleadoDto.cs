namespace Menu.DTOs.Reportes;

public class ReporteEmpleadoDto
{
    public int TrabajadorId { get; set; }

    public string Dni { get; set; } = string.Empty;
    public string Trabajador { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public int TotalMenus { get; set; }
    public int TotalAlmuerzos { get; set; }
    public int TotalCenas { get; set; }

    public decimal TotalEmpresa { get; set; }
    public decimal TotalPlanilla { get; set; }
    public decimal TotalPagoDirecto { get; set; }
    public decimal TotalCreditoPendiente { get; set; }
    public decimal TotalCreditoPagado { get; set; }

    public decimal TotalMenuExtra { get; set; }
    public decimal TotalProductos { get; set; }

    public decimal TotalExtrasProductos { get; set; }

    public decimal TotalCobrado { get; set; }
    public decimal TotalPendiente { get; set; }

    public decimal TotalGeneral { get; set; }

    public List<ReporteEmpleadoFechaDto> Detalles { get; set; } = new();
}