using Menu.DTOs;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly AuthStateService _authStateService;
    private readonly PlaceholderPageViewModel _usuariosPage = new(
        "Usuarios",
        "Administracion de usuarios, roles y permisos.");
    private readonly PlaceholderPageViewModel _configuracionPage = new(
        "Configuracion",
        "Parametros de menu, precios y comportamiento general.");

    private bool _isAuthenticated;
    private bool _isNavigationCollapsed;
    private string _currentView = "Dashboard";
    private string _usuarioActual = string.Empty;
    private object? _currentPage;

    public MainViewModel(
        LoginViewModel login,
        DashboardViewModel dashboard,
        EmpleadosViewModel empleados,
        RegistroDiarioViewModel registroDiario,
        CuentasPorCobrarViewModel cuentasPorCobrar,
        ReportesViewModel reportes,
        CierresViewModel cierres,
        AuthStateService authStateService)
    {
        _authStateService = authStateService;
        Login = login;
        Dashboard = dashboard;
        Empleados = empleados;
        RegistroDiario = registroDiario;
        CuentasPorCobrar = cuentasPorCobrar;
        Reportes = reportes;
        Cierres = cierres;
        CurrentPage = Dashboard;

        ShowDashboardCommand = new RelayCommand(ShowDashboard);
        ShowEmpleadosCommand = new RelayCommand(ShowEmpleados);
        ShowRegistroDiarioCommand = new RelayCommand(ShowRegistroDiario);
        ShowCuentasPorCobrarCommand = new RelayCommand(ShowCuentasPorCobrar);
        ShowReportesCommand = new RelayCommand(ShowReportes);
        ShowCierresCommand = new RelayCommand(ShowCierres);
        ShowUsuariosCommand = new RelayCommand(() => ShowPlaceholder("Usuarios", _usuariosPage));
        ShowConfiguracionCommand = new RelayCommand(() => ShowPlaceholder("Configuracion", _configuracionPage));
        ToggleNavigationCommand = new RelayCommand(ToggleNavigation);
        LogoutCommand = new RelayCommand(Logout, () => IsAuthenticated);
        Login.LoginSucceeded += OnLoginSucceeded;
    }

    public string Title => "FQSoft Menu - Escritorio";

    public string AppName => "FQSoft Menu";

    public string ShellStatus => IsAuthenticated
        ? $"Sesion: {UsuarioActual}"
        : "Desktop WPF";

    public string CurrentViewTitle => CurrentView switch
    {
        "Empleados" => "Comensales",
        "RegistroDiario" => "Registro diario",
        "CuentasPorCobrar" => "Cuentas por cobrar",
        "Reportes" => "Reportes",
        "Cierres" => "Cierres",
        "Usuarios" => "Usuarios",
        "Configuracion" => "Configuracion",
        _ => "Dashboard"
    };

    public string CurrentViewSubtitle => CurrentView switch
    {
        "Empleados" => "Comensales y beneficios",
        "RegistroDiario" => "Menu principal, menus extra y productos adicionales",
        "CuentasPorCobrar" => "Saldos pendientes del comedor",
        "Reportes" => "Consultas y resumenes",
        "Cierres" => "Validacion diaria",
        "Usuarios" => "Roles y accesos",
        "Configuracion" => "Parametros del sistema",
        _ => "Resumen diario del comedor"
    };

    public string NavigationWidth => IsNavigationCollapsed ? "48" : "190";

    public bool IsNavigationExpanded => !IsNavigationCollapsed;

    public string DashboardNavBackground => NavBackground("Dashboard");
    public string EmpleadosNavBackground => NavBackground("Empleados");
    public string RegistroNavBackground => NavBackground("RegistroDiario");
    public string CuentasNavBackground => NavBackground("CuentasPorCobrar");
    public string ReportesNavBackground => NavBackground("Reportes");
    public string CierresNavBackground => NavBackground("Cierres");
    public string UsuariosNavBackground => NavBackground("Usuarios");
    public string ConfiguracionNavBackground => NavBackground("Configuracion");

    public string DashboardNavForeground => NavForeground("Dashboard");
    public string EmpleadosNavForeground => NavForeground("Empleados");
    public string RegistroNavForeground => NavForeground("RegistroDiario");
    public string CuentasNavForeground => NavForeground("CuentasPorCobrar");
    public string ReportesNavForeground => NavForeground("Reportes");
    public string CierresNavForeground => NavForeground("Cierres");
    public string UsuariosNavForeground => NavForeground("Usuarios");
    public string ConfiguracionNavForeground => NavForeground("Configuracion");

    public LoginViewModel Login { get; }

    public DashboardViewModel Dashboard { get; }

    public EmpleadosViewModel Empleados { get; }

    public RegistroDiarioViewModel RegistroDiario { get; }

    public CuentasPorCobrarViewModel CuentasPorCobrar { get; }

    public ReportesViewModel Reportes { get; }

    public CierresViewModel Cierres { get; }

    public object? CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public RelayCommand ShowDashboardCommand { get; }

    public RelayCommand ShowEmpleadosCommand { get; }

    public RelayCommand ShowRegistroDiarioCommand { get; }

    public RelayCommand ShowCuentasPorCobrarCommand { get; }

    public RelayCommand ShowReportesCommand { get; }

    public RelayCommand ShowCierresCommand { get; }

    public RelayCommand ShowUsuariosCommand { get; }

    public RelayCommand ShowConfiguracionCommand { get; }

    public RelayCommand ToggleNavigationCommand { get; }

    public RelayCommand LogoutCommand { get; }

    public bool IsNavigationCollapsed
    {
        get => _isNavigationCollapsed;
        private set
        {
            if (SetProperty(ref _isNavigationCollapsed, value))
            {
                OnPropertyChanged(nameof(NavigationWidth));
                OnPropertyChanged(nameof(IsNavigationExpanded));
            }
        }
    }

    public string CurrentView
    {
        get => _currentView;
        private set
        {
            if (SetProperty(ref _currentView, value))
            {
                OnPropertyChanged(nameof(CurrentViewTitle));
                OnPropertyChanged(nameof(CurrentViewSubtitle));
                NotifyNavigationStateChanged();
            }
        }
    }

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set
        {
            if (SetProperty(ref _isAuthenticated, value))
            {
                OnPropertyChanged(nameof(ShellStatus));
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string UsuarioActual
    {
        get => _usuarioActual;
        private set
        {
            if (SetProperty(ref _usuarioActual, value))
                OnPropertyChanged(nameof(ShellStatus));
        }
    }

    private async void OnLoginSucceeded(UsuarioSesionDto usuario)
    {
        UsuarioActual = $"{usuario.NombreCompleto} - {usuario.RolNombre}";
        IsAuthenticated = true;
        CurrentView = "Dashboard";
        CurrentPage = Dashboard;
        await Dashboard.LoadAsync();
    }

    private async void ShowDashboard()
    {
        CurrentView = "Dashboard";
        CurrentPage = Dashboard;
        await Dashboard.LoadAsync();
    }

    private async void ShowEmpleados()
    {
        CurrentView = "Empleados";
        CurrentPage = Empleados;
        await Empleados.LoadAsync();
    }

    private async void ShowRegistroDiario()
    {
        CurrentView = "RegistroDiario";
        CurrentPage = RegistroDiario;
        await RegistroDiario.LoadAsync();
    }

    private async void ShowCuentasPorCobrar()
    {
        CurrentView = "CuentasPorCobrar";
        CurrentPage = CuentasPorCobrar;
        await CuentasPorCobrar.LoadAsync();
    }

    private async void ShowReportes()
    {
        CurrentView = "Reportes";
        CurrentPage = Reportes;
        await Reportes.LoadAsync();
    }

    private async void ShowCierres()
    {
        CurrentView = "Cierres";
        CurrentPage = Cierres;
        await Cierres.LoadAsync();
    }

    private void ShowPlaceholder(string viewKey, PlaceholderPageViewModel page)
    {
        CurrentView = viewKey;
        CurrentPage = page;
    }

    private void Logout()
    {
        _authStateService.Logout();
        UsuarioActual = string.Empty;
        CurrentView = "Dashboard";
        CurrentPage = Dashboard;
        IsAuthenticated = false;
    }

    private void ToggleNavigation()
    {
        IsNavigationCollapsed = !IsNavigationCollapsed;
    }

    private string NavBackground(string viewKey)
    {
        return CurrentView == viewKey ? "#EEEEEE" : "Transparent";
    }

    private string NavForeground(string viewKey)
    {
        return CurrentView == viewKey ? "#594AE2" : "#666666";
    }

    private void NotifyNavigationStateChanged()
    {
        OnPropertyChanged(nameof(DashboardNavBackground));
        OnPropertyChanged(nameof(EmpleadosNavBackground));
        OnPropertyChanged(nameof(RegistroNavBackground));
        OnPropertyChanged(nameof(CuentasNavBackground));
        OnPropertyChanged(nameof(ReportesNavBackground));
        OnPropertyChanged(nameof(CierresNavBackground));
        OnPropertyChanged(nameof(UsuariosNavBackground));
        OnPropertyChanged(nameof(ConfiguracionNavBackground));
        OnPropertyChanged(nameof(DashboardNavForeground));
        OnPropertyChanged(nameof(EmpleadosNavForeground));
        OnPropertyChanged(nameof(RegistroNavForeground));
        OnPropertyChanged(nameof(CuentasNavForeground));
        OnPropertyChanged(nameof(ReportesNavForeground));
        OnPropertyChanged(nameof(CierresNavForeground));
        OnPropertyChanged(nameof(UsuariosNavForeground));
        OnPropertyChanged(nameof(ConfiguracionNavForeground));
    }
}
