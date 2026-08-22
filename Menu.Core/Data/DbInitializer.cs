using Menu.Enums;
using Menu.Models;
using Menu.Security;
using Menu.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Menu.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        bool seedDemoData,
        bool allowAdminReset = true)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        await SeedTiposEmpleadoAsync(context);
        await SeedEmpresasClienteAsync(context);
        await SeedConfiguracionMenuAsync(context);
        if (seedDemoData)
            await SeedEmpleadosDemoAsync(context);

        await SeedRolesPermisosAsync(context);
        await SeedUsuarioAdminAsync(context, scope.ServiceProvider, allowAdminReset);
    }

    private static async Task SeedEmpresasClienteAsync(AppDbContext context)
    {
        var empresa = await context.EmpresasCliente
            .FirstOrDefaultAsync(x => x.NombreComercial == "Empresa cliente general");

        if (empresa is null)
        {
            empresa = new EmpresaCliente
            {
                NombreComercial = "Empresa cliente general",
                RazonSocial = "Empresa cliente general",
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            context.EmpresasCliente.Add(empresa);
            await context.SaveChangesAsync();
        }

        var existeSucursal = await context.Sucursales
            .AnyAsync(x => x.Nombre == "Sucursal principal");

        if (!existeSucursal)
        {
            context.Sucursales.Add(new Sucursal
            {
                Nombre = "Sucursal principal",
                EmpresaClienteId = empresa.Id,
                Activo = true,
                FechaCreacion = DateTime.Now
            });

            await context.SaveChangesAsync();
        }

        var sucursal = await context.Sucursales
            .FirstOrDefaultAsync(x => x.Nombre == "Sucursal principal");

        var empleadosSinEmpresa = await context.Empleados
            .Where(x => x.EmpresaClienteId == null)
            .ToListAsync();

        foreach (var empleado in empleadosSinEmpresa)
        {
            empleado.EmpresaClienteId = empresa.Id;
            empleado.SucursalId ??= sucursal?.Id;
        }

        if (empleadosSinEmpresa.Count > 0)
            await context.SaveChangesAsync();
    }

    private static async Task SeedTiposEmpleadoAsync(AppDbContext context)
    {
        var tiposBase = new[]
        {
            "Empleado",
            "Obrero",
            "Tercero",
            "Practicante",
            "Visitante",
            "Gerencia",
            "Otro"
        };

        foreach (var nombre in tiposBase)
        {
            var existe = await context.TiposEmpleado
                .AnyAsync(x => x.Nombre == nombre);

            if (!existe)
            {
                context.TiposEmpleado.Add(new TipoEmpleado
                {
                    Nombre = nombre,
                    Activo = true,
                    FechaCreacion = DateTime.Now
                });
            }
        }

        await context.SaveChangesAsync();

        var tipos = await context.TiposEmpleado.ToDictionaryAsync(x => x.Nombre);
        var empleadosSinTipo = await context.Empleados
            .Where(x => x.TipoEmpleadoId == null)
            .ToListAsync();

        foreach (var empleado in empleadosSinTipo)
        {
            var nombreTipo = empleado.Categoria.ToString();
            empleado.TipoEmpleadoId = tipos.TryGetValue(nombreTipo, out var tipo)
                ? tipo.Id
                : tipos["Obrero"].Id;
        }

        if (empleadosSinTipo.Count > 0)
            await context.SaveChangesAsync();
    }

    private static async Task SeedConfiguracionMenuAsync(AppDbContext context)
    {
        var existeConfiguracion = await context.ConfiguracionesMenu.AnyAsync();

        if (existeConfiguracion)
            return;

        context.ConfiguracionesMenu.Add(new ConfiguracionMenu
        {
            PrecioMenu = 12.00m,
            Moneda = "PEN",
            FechaActualizacion = DateTime.Now
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedEmpleadosDemoAsync(AppDbContext context)
    {
        var existeEmpleado = await context.Empleados.AnyAsync();

        if (existeEmpleado)
            return;

        context.Empleados.AddRange(
            new Empleado
            {
                Dni = "12345678",
                Nombres = "Juan",
                Apellidos = "Pérez",
                TipoEmpleadoId = await GetTipoEmpleadoIdAsync(context, "Obrero"),
                EmpresaClienteId = await GetEmpresaClienteIdAsync(context, "Empresa cliente general"),
                SucursalId = await GetSucursalIdAsync(context, "Sucursal principal"),
                Estado = EstadoEmpleado.Activo,
                Activo = true,
                FechaCreacion = DateTime.Now
            },
            new Empleado
            {
                Dni = "87654321",
                Nombres = "María",
                Apellidos = "López",
                TipoEmpleadoId = await GetTipoEmpleadoIdAsync(context, "Obrero"),
                EmpresaClienteId = await GetEmpresaClienteIdAsync(context, "Empresa cliente general"),
                SucursalId = await GetSucursalIdAsync(context, "Sucursal principal"),
                Estado = EstadoEmpleado.Suspendido,
                Activo = true,
                FechaCreacion = DateTime.Now
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task<int?> GetTipoEmpleadoIdAsync(AppDbContext context, string nombre)
    {
        return await context.TiposEmpleado
            .Where(x => x.Nombre == nombre)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task<int?> GetEmpresaClienteIdAsync(AppDbContext context, string nombre)
    {
        return await context.EmpresasCliente
            .Where(x => x.NombreComercial == nombre)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task<int?> GetSucursalIdAsync(AppDbContext context, string nombre)
    {
        return await context.Sucursales
            .Where(x => x.Nombre == nombre)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync();
    }

    private static async Task SeedRolesPermisosAsync(AppDbContext context)
    {
        var permisosBase = new List<PermisoSistema>
    {
        new() { Codigo = Permisos.DashboardVer, Nombre = "Ver dashboard", Modulo = "Inicio" },

        new() { Codigo = Permisos.EmpleadosVer, Nombre = "Ver comensales", Modulo = "Comensales" },
        new() { Codigo = Permisos.EmpleadosCrear, Nombre = "Crear comensales", Modulo = "Comensales" },
        new() { Codigo = Permisos.EmpleadosEditar, Nombre = "Editar comensales", Modulo = "Comensales" },

        new() { Codigo = Permisos.RegistroDiarioVer, Nombre = "Ver registro diario", Modulo = "Registro diario" },
        new() { Codigo = Permisos.RegistroDiarioRegistrar, Nombre = "Registrar consumos", Modulo = "Registro diario" },

        new() { Codigo = Permisos.CuentasCobrarVer, Nombre = "Ver cuentas por cobrar", Modulo = "Cuentas por cobrar" },
        new() { Codigo = Permisos.CuentasCobrarPagar, Nombre = "Registrar pagos", Modulo = "Cuentas por cobrar" },

        new() { Codigo = Permisos.ReportesVer, Nombre = "Ver reportes", Modulo = "Reportes" },

        new() { Codigo = Permisos.CierresVer, Nombre = "Ver cierres", Modulo = "Cierres" },
        new() { Codigo = Permisos.CierresGestionar, Nombre = "Gestionar cierres", Modulo = "Cierres" },

        new() { Codigo = Permisos.UsuariosVer, Nombre = "Ver usuarios", Modulo = "Usuarios" },
        new() { Codigo = Permisos.UsuariosCrear, Nombre = "Crear usuarios", Modulo = "Usuarios" },
        new() { Codigo = Permisos.UsuariosEditar, Nombre = "Editar usuarios", Modulo = "Usuarios" },

        new() { Codigo = Permisos.ConfiguracionVer, Nombre = "Ver configuración", Modulo = "Configuración" },
        new() { Codigo = Permisos.ConfiguracionEditar, Nombre = "Editar configuración", Modulo = "Configuración" }
    };

        foreach (var permiso in permisosBase)
        {
            var existe = await context.PermisosSistema
                .AnyAsync(x => x.Codigo == permiso.Codigo);

            if (!existe)
            {
                context.PermisosSistema.Add(permiso);
            }
        }

        await context.SaveChangesAsync();

        var rolesBase = new List<RolSistema>
    {
        new() { Codigo = "ADMIN", Nombre = "Administrador", Activo = true, FechaCreacion = DateTime.Now },
        new() { Codigo = "ENCARGADO_COMEDOR", Nombre = "Encargado comedor", Activo = true, FechaCreacion = DateTime.Now },
        new() { Codigo = "ADMIN_FACTURACION", Nombre = "Administracion / Facturacion", Activo = true, FechaCreacion = DateTime.Now },
        new() { Codigo = "CONSULTA", Nombre = "Consulta", Activo = true, FechaCreacion = DateTime.Now }
    };

        foreach (var rol in rolesBase)
        {
            var existe = await context.RolesSistema
                .AnyAsync(x => x.Codigo == rol.Codigo);

            if (!existe)
            {
                context.RolesSistema.Add(rol);
            }
        }

        await context.SaveChangesAsync();

        await AsignarPermisosRolAsync(context, "ADMIN", permisosBase.Select(x => x.Codigo).ToArray());

        await AsignarPermisosRolAsync(context, "ENCARGADO_COMEDOR",
            Permisos.DashboardVer,
            Permisos.RegistroDiarioVer,
            Permisos.RegistroDiarioRegistrar,
            Permisos.CuentasCobrarVer,
            Permisos.CuentasCobrarPagar,
            Permisos.ReportesVer,
            Permisos.CierresVer);

        await AsignarPermisosRolAsync(context, "ADMIN_FACTURACION",
            Permisos.DashboardVer,
            Permisos.EmpleadosVer,
            Permisos.CuentasCobrarVer,
            Permisos.CuentasCobrarPagar,
            Permisos.ReportesVer,
            Permisos.CierresVer,
            Permisos.CierresGestionar);

        await AsignarPermisosRolAsync(context, "CONSULTA",
            Permisos.DashboardVer,
            Permisos.ReportesVer,
            Permisos.CierresVer);
    }

    private static async Task AsignarPermisosRolAsync(
    AppDbContext context,
    string codigoRol,
    params string[] codigosPermisos)
    {
        var rol = await context.RolesSistema
            .FirstOrDefaultAsync(x => x.Codigo == codigoRol);

        if (rol is null)
            return;

        var permisos = await context.PermisosSistema
            .Where(x => codigosPermisos.Contains(x.Codigo))
            .ToListAsync();

        foreach (var permiso in permisos)
        {
            var existe = await context.RolesPermisos
                .AnyAsync(x => x.RolSistemaId == rol.Id &&
                               x.PermisoSistemaId == permiso.Id);

            if (!existe)
            {
                context.RolesPermisos.Add(new RolPermiso
                {
                    RolSistemaId = rol.Id,
                    PermisoSistemaId = permiso.Id
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedUsuarioAdminAsync(
        AppDbContext context,
        IServiceProvider serviceProvider,
        bool allowAdminReset)
    {
        var rolAdmin = await context.RolesSistema
            .FirstOrDefaultAsync(x => x.Codigo == "ADMIN");

        if (rolAdmin is null)
            return;

        var adminPassword = Environment.GetEnvironmentVariable("FQSOFT_ADMIN_PASSWORD");
        var resetAdmin = allowAdminReset && string.Equals(
            Environment.GetEnvironmentVariable("FQSOFT_RESET_ADMIN_PASSWORD"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = "admin123";
        }

        var passwordService = serviceProvider.GetRequiredService<PasswordService>();
        var adminExistente = await context.UsuariosSistema
            .FirstOrDefaultAsync(x => x.NombreUsuario == "admin");

        if (adminExistente is not null)
        {
            if (!resetAdmin)
                return;

            adminExistente.ClaveHash = passwordService.HashPassword(adminPassword);
            adminExistente.RolSistemaId = rolAdmin.Id;
            adminExistente.Activo = true;

            await context.SaveChangesAsync();
            return;
        }

        var existeUsuario = await context.UsuariosSistema.AnyAsync();

        if (existeUsuario && !resetAdmin)
            return;

        context.UsuariosSistema.Add(new UsuarioSistema
        {
            NombreUsuario = "admin",
            NombreCompleto = "Administrador",
            ClaveHash = passwordService.HashPassword(adminPassword),
            RolSistemaId = rolAdmin.Id,
            Activo = true,
            FechaCreacion = DateTime.Now
        });

        await context.SaveChangesAsync();
    }
}
