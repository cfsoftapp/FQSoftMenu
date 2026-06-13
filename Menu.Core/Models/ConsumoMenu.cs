using Menu.Enums;

namespace Menu.Models;

public class ConsumoMenu
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    public TipoServicioMenu TipoServicio { get; set; } = TipoServicioMenu.Almuerzo;

    public int EmpleadoId { get; set; }

    public Empleado Empleado { get; set; } = null!;

    public decimal PrecioMenu { get; set; }

    public TipoPagoMenu TipoPagoMenu { get; set; }

    public FormaPago? FormaPagoDirecto { get; set; }

    public EstadoCobroAdicional? EstadoCobroMenu { get; set; }

    public DateTime? FechaPagoMenu { get; set; }

    public string? Observacion { get; set; }

    public int UsuarioRegistroId { get; set; }

    public string UsuarioRegistroNombre { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public bool Anulado { get; set; } = false;

    public DateTime? FechaAnulacion { get; set; }

    public int? UsuarioAnulacionId { get; set; }

    public string? UsuarioAnulacionNombre { get; set; }

    public string? MotivoAnulacion { get; set; }

    public MotivoAnulacionConsumo? TipoMotivoAnulacion { get; set; }

    public string? ObservacionAnulacion { get; set; }

    public ICollection<ConsumoAdicional> ConsumosAdicionales { get; set; } = new List<ConsumoAdicional>();
}