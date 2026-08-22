using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Menu.Data;
using Menu.DependencyInjection;
using Menu.Desktop.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;

namespace Menu.Desktop;

public partial class App : Application
{
    private readonly IHost _host;
    private IServiceScope? _applicationScope;

    public App()
    {
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var configuredEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var environmentName = !string.IsNullOrWhiteSpace(configuredEnvironment)
            ? configuredEnvironment
            : Debugger.IsAttached
                ? Environments.Development
                : Environments.Production;

        _host = Host.CreateDefaultBuilder()
            .UseEnvironment(environmentName)
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((context, configuration) =>
            {
                configuration.SetBasePath(AppContext.BaseDirectory);
                configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                configuration.AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true,
                    reloadOnChange: true);
                configuration.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("No se configuro la conexion DefaultConnection.");
                var sqlite = new SqliteConnectionStringBuilder(connectionString);

                if (!Path.IsPathRooted(sqlite.DataSource))
                {
                    sqlite.DataSource = Path.GetFullPath(
                        Path.Combine(AppContext.BaseDirectory, sqlite.DataSource));
                }

                var databaseDirectory = Path.GetDirectoryName(sqlite.DataSource);
                if (!string.IsNullOrWhiteSpace(databaseDirectory))
                    Directory.CreateDirectory(databaseDirectory);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(sqlite.ToString()));

                services.AddMenuCoreServices();
                services.AddScoped<LoginViewModel>();
                services.AddScoped<DashboardViewModel>();
                services.AddScoped<EmpleadosViewModel>();
                services.AddScoped<RegistroDiarioViewModel>();
                services.AddScoped<CuentasPorCobrarViewModel>();
                services.AddScoped<ReportesViewModel>();
                services.AddScoped<CierresViewModel>();
                services.AddScoped<UsuariosViewModel>();
                services.AddScoped<ConfiguracionViewModel>();
                services.AddScoped<MainViewModel>();
                services.AddScoped<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            await _host.StartAsync();
            var hostEnvironment = _host.Services.GetRequiredService<IHostEnvironment>();
            await DbInitializer.InitializeAsync(
                _host.Services,
                seedDemoData: false,
                allowAdminReset: !hostEnvironment.IsDevelopment());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo preparar la base de datos:{Environment.NewLine}{ex.Message}",
                "FQSoft Menu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        _applicationScope = _host.Services.CreateScope();
        var mainWindow = _applicationScope.ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        _applicationScope?.Dispose();
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FQSoft",
                "Menu",
                "desktop-errors.log");

            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            File.AppendAllText(
                logPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}{e.Exception}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Error logging must never terminate the desktop app.
        }

        MessageBox.Show(
            $"Ocurrio un error en la aplicacion:{Environment.NewLine}{e.Exception.Message}",
            "FQSoft Menu",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}
