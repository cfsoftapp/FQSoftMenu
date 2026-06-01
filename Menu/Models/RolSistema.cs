namespace Menu.Models;

public class RolSistema
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public ICollection<UsuarioSistema> Usuarios { get; set; } = new List<UsuarioSistema>();

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}