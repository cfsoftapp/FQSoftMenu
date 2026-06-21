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

        public int? TipoEmpleadoId { get; set; }

        public TipoEmpleado? TipoEmpleado { get; set; }

        public string TipoEmpleadoNombre => TipoEmpleado?.Nombre ?? Categoria.ToString();

        public int? EmpresaClienteId { get; set; }

        public EmpresaCliente? EmpresaCliente { get; set; }

        public string EmpresaClienteNombre => EmpresaCliente?.NombreComercial ?? "Sin empresa";

        public int? SucursalId { get; set; }

        public Sucursal? Sucursal { get; set; }

        public string SucursalNombre => Sucursal?.Nombre ?? "Sin sucursal";

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public ICollection<ConsumoMenu> ConsumosMenu { get; set; } = new List<ConsumoMenu>();

        public ICollection<ConsumoAdicional> ConsumosAdicionales { get; set; } = new List<ConsumoAdicional>();

        public ICollection<PagoConsumoAdicional> PagosAdicionales { get; set; } = new List<PagoConsumoAdicional>();
    }
}
