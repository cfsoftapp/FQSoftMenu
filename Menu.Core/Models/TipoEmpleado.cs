namespace Menu.Models;

public class TipoEmpleado
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
