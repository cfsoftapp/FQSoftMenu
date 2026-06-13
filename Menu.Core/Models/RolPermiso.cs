namespace Menu.Models;

public class RolPermiso
{
    public int Id { get; set; }

    public int RolSistemaId { get; set; }

    public RolSistema RolSistema { get; set; } = null!;

    public int PermisoSistemaId { get; set; }

    public PermisoSistema PermisoSistema { get; set; } = null!;
}