namespace Menu.Security;

public static class Permisos
{
    public const string DashboardVer = "DASHBOARD_VER";

    public const string EmpleadosVer = "EMPLEADOS_VER";
    public const string EmpleadosCrear = "EMPLEADOS_CREAR";
    public const string EmpleadosEditar = "EMPLEADOS_EDITAR";

    public const string RegistroDiarioVer = "REGISTRO_DIARIO_VER";
    public const string RegistroDiarioRegistrar = "REGISTRO_DIARIO_REGISTRAR";

    public const string CuentasCobrarVer = "CUENTAS_COBRAR_VER";
    public const string CuentasCobrarPagar = "CUENTAS_COBRAR_PAGAR";

    public const string ReportesVer = "REPORTES_VER";

    public const string CierresVer = "CIERRES_VER";
    public const string CierresGestionar = "CIERRES_GESTIONAR";

    public const string UsuariosVer = "USUARIOS_VER";
    public const string UsuariosCrear = "USUARIOS_CREAR";
    public const string UsuariosEditar = "USUARIOS_EDITAR";

    public const string ConfiguracionVer = "CONFIGURACION_VER";
    public const string ConfiguracionEditar = "CONFIGURACION_EDITAR";

    public static readonly string[] Todos =
    {
        DashboardVer,
        EmpleadosVer,
        EmpleadosCrear,
        EmpleadosEditar,
        RegistroDiarioVer,
        RegistroDiarioRegistrar,
        CuentasCobrarVer,
        CuentasCobrarPagar,
        ReportesVer,
        CierresVer,
        CierresGestionar,
        UsuariosVer,
        UsuariosCrear,
        UsuariosEditar,
        ConfiguracionVer,
        ConfiguracionEditar
    };
}
