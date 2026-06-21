using Menu.Services;
using Menu.Services.Cierres;
using Menu.Services.Reportes;
using Microsoft.Extensions.DependencyInjection;

namespace Menu.DependencyInjection;

public static class MenuCoreServiceCollectionExtensions
{
    public static IServiceCollection AddMenuCoreServices(this IServiceCollection services)
    {
        services.AddScoped<EmpleadoService>();
        services.AddScoped<TipoEmpleadoService>();
        services.AddScoped<EmpresaClienteService>();
        services.AddScoped<SucursalService>();
        services.AddScoped<ConfiguracionMenuService>();
        services.AddScoped<RegistroDiarioService>();
        services.AddScoped<CuentaPorCobrarService>();
        services.AddScoped<PasswordService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<AuthStateService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<ICierreService, CierreService>();

        return services;
    }
}
