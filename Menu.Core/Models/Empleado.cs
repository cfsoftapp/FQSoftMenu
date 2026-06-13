using Menu.Enums;

namespace Menu.Models
{
    public class Empleado
    {
        public int Id { get; set; }

        public string Dni { get; set; } = string.Empty;

        public string Nombres { get; set; } = string.Empty;

        public string Apellidos { get; set; } = string.Empty;

        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

        public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

        public CategoriaEmpleado Categoria { get; set; } = CategoriaEmpleado.Obrero;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<ConsumoMenu> ConsumosMenu { get; set; } = new List<ConsumoMenu>();

        public ICollection<ConsumoAdicional> ConsumosAdicionales { get; set; } = new List<ConsumoAdicional>();

        public ICollection<PagoConsumoAdicional> PagosAdicionales { get; set; } = new List<PagoConsumoAdicional>();
    }
}
