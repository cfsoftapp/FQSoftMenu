using Menu.Enums;

namespace Menu.Models;

public class UsuarioSistema
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string ClaveHash { get; set; } = string.Empty;

    public int RolSistemaId { get; set; }

    public RolSistema RolSistema { get; set; } = null!;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}