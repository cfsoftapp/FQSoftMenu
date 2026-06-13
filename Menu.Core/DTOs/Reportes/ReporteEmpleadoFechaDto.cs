namespace Menu.DTOs.Reportes;

public class ReporteEmpleadoFechaDto
{
    public DateTime FechaConsumo { get; set; }

    public bool ConsumioMenuPrincipal { get; set; }

    public string EstadoTrabajador { get; set; } = string.Empty;
    public string TipoPagoMenuPrincipal { get; set; } = string.Empty;
    public string MedioPago { get; set; } = string.Empty;

    public decimal ImporteEmpresa { get; set; }
    public decimal ImportePlanilla { get; set; }
    public decimal ImportePagoDirecto { get; set; }
    public decimal ImporteCreditoPendiente { get; set; }
    public decimal ImporteCreditoPagado { get; set; }

    public decimal ImporteMenuExtra { get; set; }
    public decimal ImporteProductos { get; set; }

    public decimal TotalCobrado { get; set; }
    public decimal TotalPendiente { get; set; }

    public string Observacion { get; set; } = string.Empty;

    public List<ReporteAdicionalDetalleDto> Adicionales { get; set; } = new();
}