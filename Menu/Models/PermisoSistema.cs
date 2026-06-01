namespace Menu.Models;

public class PermisoSistema
{
    public int Id { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Modulo { get; set; } = string.Empty;

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}