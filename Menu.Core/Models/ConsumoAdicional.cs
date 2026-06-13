using Menu.Enums;

namespace Menu.Models
{
    public class ConsumoAdicional
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Today;

        public int EmpleadoId { get; set; }

        public Empleado Empleado { get; set; } = null!;

        public int? ConsumoMenuId { get; set; }

        public ConsumoMenu? ConsumoMenu { get; set; }

        public TipoAdicional TipoAdicional { get; set; } = TipoAdicional.Producto;

        public CategoriaConsumoAdicional Categoria { get; set; } = CategoriaConsumoAdicional.Otro;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public FormaCobroAdicional FormaCobro { get; set; }

        public EstadoCobroAdicional EstadoCobro { get; set; }

        public DateTime? FechaPago { get; set; }

        public string? Observacion { get; set; }

        public int UsuarioRegistroId { get; set; }

        public string UsuarioRegistroNombre { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public bool Anulado { get; set; } = false;

        public DateTime? FechaAnulacion { get; set; }

        public int? UsuarioAnulacionId { get; set; }

        public string? UsuarioAnulacionNombre { get; set; }

        public string? MotivoAnulacion { get; set; }

        public MotivoAnulacionConsumo? MotivoAnulacionConsumo { get; set; }

        public string? ObservacionAnulacion { get; set; }

        public ICollection<PagoConsumoAdicionalDetalle> PagoDetalles { get; set; } = new List<PagoConsumoAdicionalDetalle>();
    }
}