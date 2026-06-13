using Menu.Enums;

namespace Menu.DTOs;

public class EmpleadoListDto
{
    public int Id { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public CategoriaEmpleado Categoria { get; set; }

    public EstadoEmpleado Estado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}