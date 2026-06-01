using Menu.Data;
using Menu.DTOs;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class UsuarioService
{
    private readonly AppDbContext _context;
    private readonly PasswordService _passwordService;

    public UsuarioService(AppDbContext context, PasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<UsuarioSesionDto?> LoginAsync(LoginInputDto input)
    {
        var username = input.NombreUsuario.Trim().ToLower();

        var usuario = await _context.UsuariosSistema
            .Include(x => x.RolSistema)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.PermisoSistema)
            .FirstOrDefaultAsync(x => x.NombreUsuario.ToLower() == username && x.Activo);

        if (usuario is null)
            return null;

        if (!usuario.RolSistema.Activo)
            return null;

        var claveValida = _passwordService.VerifyPassword(input.Clave, usuario.ClaveHash);

        if (!claveValida)
            return null;

        if (_passwordService.NeedsRehash(usuario.ClaveHash))
        {
            usuario.ClaveHash = _passwordService.HashPassword(input.Clave);
            await _context.SaveChangesAsync();
        }

        return new UsuarioSesionDto
        {
            Id = usuario.Id,
            NombreUsuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            RolSistemaId = usuario.RolSistemaId,
            RolCodigo = usuario.RolSistema.Codigo,
            RolNombre = usuario.RolSistema.Nombre,
            Permisos = usuario.RolSistema.RolPermisos
                .Select(x => x.PermisoSistema.Codigo)
                .Distinct()
                .ToList(),
            EstaAutenticado = true
        };
    }

    public async Task<List<UsuarioSistema>> GetAllAsync()
    {
        return await _context.UsuariosSistema
            .Include(x => x.RolSistema)
            .OrderBy(x => x.NombreCompleto)
            .ToListAsync();
    }

    public async Task<UsuarioSistema?> GetByIdAsync(int id)
    {
        return await _context.UsuariosSistema
            .Include(x => x.RolSistema)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<RolSistema>> GetRolesActivosAsync()
    {
        return await _context.RolesSistema
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excludeId = null)
    {
        nombreUsuario = nombreUsuario.Trim().ToLower();

        return await _context.UsuariosSistema
            .AnyAsync(x =>
                x.NombreUsuario.ToLower() == nombreUsuario &&
                (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    public async Task<(bool Success, string Message)> CrearUsuarioAsync(
        string nombreUsuario,
        string nombreCompleto,
        string clave,
        int rolSistemaId)
    {
        nombreUsuario = nombreUsuario.Trim().ToLower();
        nombreCompleto = nombreCompleto.Trim();

        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return (false, "El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombreCompleto))
            return (false, "El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(clave))
            return (false, "La clave es obligatoria.");

        if (clave.Length < 6)
            return (false, "La clave debe tener al menos 6 caracteres.");

        if (rolSistemaId <= 0)
            return (false, "Debe seleccionar un rol.");

        var rolExiste = await _context.RolesSistema
            .AnyAsync(x => x.Id == rolSistemaId && x.Activo);

        if (!rolExiste)
            return (false, "El rol seleccionado no existe o está inactivo.");

        if (await ExisteNombreUsuarioAsync(nombreUsuario))
            return (false, "Ya existe un usuario con ese nombre.");

        var usuario = new UsuarioSistema
        {
            NombreUsuario = nombreUsuario,
            NombreCompleto = nombreCompleto,
            ClaveHash = _passwordService.HashPassword(clave),
            RolSistemaId = rolSistemaId,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.UsuariosSistema.Add(usuario);
        await _context.SaveChangesAsync();

        return (true, "Usuario creado correctamente.");
    }

    public async Task<(bool Success, string Message)> ActualizarUsuarioAsync(UsuarioSistema usuario)
    {
        var dbUsuario = await _context.UsuariosSistema.FindAsync(usuario.Id);

        if (dbUsuario is null)
            return (false, "El usuario no existe.");

        usuario.NombreUsuario = usuario.NombreUsuario.Trim().ToLower();
        usuario.NombreCompleto = usuario.NombreCompleto.Trim();

        if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            return (false, "El usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
            return (false, "El nombre completo es obligatorio.");

        if (usuario.RolSistemaId <= 0)
            return (false, "Debe seleccionar un rol.");

        var rolExiste = await _context.RolesSistema
            .AnyAsync(x => x.Id == usuario.RolSistemaId && x.Activo);

        if (!rolExiste)
            return (false, "El rol seleccionado no existe o está inactivo.");

        if (await ExisteNombreUsuarioAsync(usuario.NombreUsuario, usuario.Id))
            return (false, "Ya existe otro usuario con ese nombre.");

        dbUsuario.NombreUsuario = usuario.NombreUsuario;
        dbUsuario.NombreCompleto = usuario.NombreCompleto;
        dbUsuario.RolSistemaId = usuario.RolSistemaId;
        dbUsuario.Activo = usuario.Activo;

        await _context.SaveChangesAsync();

        return (true, "Usuario actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> CambiarClaveAsync(int usuarioId, string nuevaClave)
    {
        var usuario = await _context.UsuariosSistema.FindAsync(usuarioId);

        if (usuario is null)
            return (false, "El usuario no existe.");

        if (string.IsNullOrWhiteSpace(nuevaClave))
            return (false, "La nueva clave es obligatoria.");

        if (nuevaClave.Length < 6)
            return (false, "La clave debe tener al menos 6 caracteres.");

        usuario.ClaveHash = _passwordService.HashPassword(nuevaClave);

        await _context.SaveChangesAsync();

        return (true, "Clave actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var usuario = await _context.UsuariosSistema.FindAsync(id);

        if (usuario is null)
            return (false, "El usuario no existe.");

        usuario.Activo = !usuario.Activo;

        await _context.SaveChangesAsync();

        return (true, usuario.Activo ? "Usuario activado." : "Usuario desactivado.");
    }
}
