using Menu.Enums;

namespace Menu.Models
{
    public class PagoConsumoAdicional
    {
        public int Id { get; set; }

        public int EmpleadoId { get; set; }

        public Empleado Empleado { get; set; } = null!;

        public DateTime FechaPago { get; set; } = DateTime.Now;

        public FormaPagoCredito FormaPago { get; set; } //= null!;

        public decimal MontoPagado { get; set; }

        public string? Observacion { get; set; }

        public int UsuarioRegistroId { get; set; }

        public string UsuarioRegistroNombre { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public ICollection<PagoConsumoAdicionalDetalle> Detalles { get; set; } = new List<PagoConsumoAdicionalDetalle>();
    }
}