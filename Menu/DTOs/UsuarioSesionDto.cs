namespace Menu.DTOs;

public class UsuarioSesionDto
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public int RolSistemaId { get; set; }

    public string RolCodigo { get; set; } = string.Empty;

    public string RolNombre { get; set; } = string.Empty;

    public List<string> Permisos { get; set; } = new();

    public bool EstaAutenticado { get; set; }
}