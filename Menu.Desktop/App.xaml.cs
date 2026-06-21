using System.Windows;
using Menu.Data;
using Menu.DependencyInjection;
using Menu.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Menu.Desktop;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                services.AddMenuCoreServices();
                services.AddSingleton<LoginViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<EmpleadosViewModel>();
                services.AddSingleton<RegistroDiarioViewModel>();
                services.AddSingleton<CuentasPorCobrarViewModel>();
                services.AddSingleton<ReportesViewModel>();
                services.AddSingleton<CierresViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host.Start();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
