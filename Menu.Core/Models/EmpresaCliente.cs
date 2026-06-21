namespace Menu.Models;

public class EmpresaCliente
{
    public int Id { get; set; }

    public string NombreComercial { get; set; } = string.Empty;

    public string? RazonSocial { get; set; }

    public string? Ruc { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public ICollection<Sucursal> Sucursales { get; set; } = new List<Sucursal>();

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
