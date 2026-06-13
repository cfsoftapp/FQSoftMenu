using System.Security.Claims;
using Menu.DTOs;

namespace Menu.Security;

public static class ClaimsPrincipalExtensions
{
    public static UsuarioSesionDto? ToUsuarioSesion(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return null;

        var userIdText = principal.FindFirst(AppClaimTypes.UserId)?.Value;
        var roleIdText = principal.FindFirst(AppClaimTypes.RoleId)?.Value;

        if (!int.TryParse(userIdText, out var userId) ||
            !int.TryParse(roleIdText, out var roleId))
        {
            return null;
        }

        return new UsuarioSesionDto
        {
            Id = userId,
            NombreUsuario = principal.Identity?.Name ?? string.Empty,
            NombreCompleto = principal.FindFirst(AppClaimTypes.FullName)?.Value ?? string.Empty,
            RolSistemaId = roleId,
            RolCodigo = principal.FindFirst(AppClaimTypes.RoleCode)?.Value ?? string.Empty,
            RolNombre = principal.FindFirst(AppClaimTypes.RoleName)?.Value ?? string.Empty,
            Permisos = principal.FindAll(AppClaimTypes.Permission)
                .Select(x => x.Value)
                .Distinct()
                .ToList(),
            EstaAutenticado = true
        };
    }
}
