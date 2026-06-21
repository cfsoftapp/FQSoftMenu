namespace Menu.Models;

public class Sucursal
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Direccion { get; set; }

    public int? EmpresaClienteId { get; set; }

    public EmpresaCliente? EmpresaCliente { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
