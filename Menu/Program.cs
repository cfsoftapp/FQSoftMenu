using Menu.Components;
using Menu.Data;
using Menu.DependencyInjection;
using Menu.DTOs;
using Menu.Security;
using Menu.Services;
using Menu.Services.Cierres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "FQSoftMenu.Auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in Permisos.Todos)
    {
        options.AddPolicy(permiso, policy =>
            policy.RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Permission, permiso));
    }
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(App).Assembly.FullName)));

builder.Services.AddMenuCoreServices();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services, app.Environment.IsDevelopment());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/cierres/proveedor/{id:int}/excel", async (int id, Menu.Services.Cierres.ICierreService cierreService) =>
{
    var bytes = await cierreService.GenerarExcelProveedorAsync(id);
    var fileName = $"liquidacion-proveedor-{id}.xlsx";

    return Results.File(
        bytes,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        fileName);
})
.RequireAuthorization(Permisos.CierresVer);

app.MapGet("/empleados/formato-carga.csv", () =>
{
    const string contenido = "DNI,Nombres,Apellidos,TipoPersonal,Estado,Activo\r\n" +
                             "00000000,Nombres,Apellidos,Obrero,Activo,SI\r\n";

    return Results.File(
        Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(contenido)).ToArray(),
        "text/csv; charset=utf-8",
        "formato-carga-empleados.csv");
})
.RequireAuthorization(Permisos.EmpleadosCrear);

app.MapPost("/auth/login", async (
    HttpContext httpContext,
    UsuarioService usuarioService,
    [Microsoft.AspNetCore.Mvc.FromForm] LoginInputDto input) =>
{
    var usuario = await usuarioService.LoginAsync(input);

    if (usuario is null)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(input.ReturnUrl)}");

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        usuarioService.CrearPrincipal(usuario));

    var returnUrl = EsReturnUrlLocal(input.ReturnUrl) ? input.ReturnUrl : "/";
    return Results.Redirect(returnUrl);
});

app.MapPost("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();

static bool EsReturnUrlLocal(string? returnUrl)
{
    return !string.IsNullOrWhiteSpace(returnUrl) &&
           Uri.TryCreate(returnUrl, UriKind.Relative, out _) &&
           returnUrl.StartsWith('/');
}
