using Menu.Security;

namespace Menu.Desktop.ViewModels;

public sealed class MainViewModel
{
    public string Title => "FQSoft Menu - Escritorio";

    public string AppName => "FQSoft Menu";

    public string ShellStatus => "Desktop WPF";

    public string WelcomeTitle => "Aplicacion de escritorio preparada";

    public string WelcomeMessage => "Este proyecto WPF ya referencia el nucleo compartido Menu.Core.";

    public string CoreStatus => $"Core disponible. Permisos configurados: {Permisos.Todos.Length}.";
}
