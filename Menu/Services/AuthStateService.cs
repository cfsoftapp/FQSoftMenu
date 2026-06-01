using Menu.DTOs;

namespace Menu.Services;

public class AuthStateService
{
    public UsuarioSesionDto? UsuarioActual { get; private set; }

    public bool EstaAutenticado => UsuarioActual?.EstaAutenticado == true;

    public event Action? OnChange;

    public void SetUsuario(UsuarioSesionDto usuario)
    {
        UsuarioActual = usuario;
        NotifyStateChanged();
    }

    public void Logout()
    {
        UsuarioActual = null;
        NotifyStateChanged();
    }

    public bool TienePermiso(string permiso)
    {
        return EstaAutenticado &&
               UsuarioActual is not null &&
               UsuarioActual.Permisos.Contains(permiso);
    }

    public bool TieneAlguno(params string[] permisos)
    {
        return EstaAutenticado &&
               UsuarioActual is not null &&
               permisos.Any(p => UsuarioActual.Permisos.Contains(p));
    }

    public bool EsRol(string codigoRol)
    {
        return EstaAutenticado &&
               UsuarioActual is not null &&
               UsuarioActual.RolCodigo == codigoRol;
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}