using Menu.Components;
using Menu.Data;
using Menu.Services;
using Menu.Services.Cierres;
using Menu.Services.Reportes;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<EmpleadoService>();
builder.Services.AddScoped<ConfiguracionMenuService>();
builder.Services.AddScoped<RegistroDiarioService>();
builder.Services.AddScoped<CuentaPorCobrarService>();

builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<IReporteService, ReporteService>();
builder.Services.AddScoped<ICierreService, CierreService>();

var app = builder.Build();

await DbInitializer.InitializeAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
