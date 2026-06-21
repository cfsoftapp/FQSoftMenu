using System.Windows.Input;
using Menu.DTOs;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class LoginViewModel : ObservableObject
{
    private readonly UsuarioService _usuarioService;
    private readonly AuthStateService _authStateService;
    private readonly AsyncRelayCommand _loginCommand;
    private string _nombreUsuario = "admin";
    private string _clave = string.Empty;
    private string _mensajeError = string.Empty;
    private bool _isBusy;

    public LoginViewModel(UsuarioService usuarioService, AuthStateService authStateService)
    {
        _usuarioService = usuarioService;
        _authStateService = authStateService;
        _loginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
    }

    public event Action<UsuarioSesionDto>? LoginSucceeded;

    public string NombreUsuario
    {
        get => _nombreUsuario;
        set
        {
            if (SetProperty(ref _nombreUsuario, value))
                _loginCommand.RaiseCanExecuteChanged();
        }
    }

    public string Clave
    {
        get => _clave;
        set
        {
            if (SetProperty(ref _clave, value))
                _loginCommand.RaiseCanExecuteChanged();
        }
    }

    public string MensajeError
    {
        get => _mensajeError;
        private set
        {
            if (SetProperty(ref _mensajeError, value))
                OnPropertyChanged(nameof(HasMensajeError));
        }
    }

    public bool HasMensajeError => !string.IsNullOrWhiteSpace(MensajeError);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
                _loginCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand LoginCommand => _loginCommand;

    private bool CanLogin()
    {
        return !IsBusy;
    }

    private async Task LoginAsync()
    {
        IsBusy = true;
        MensajeError = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario) ||
                string.IsNullOrWhiteSpace(Clave))
            {
                MensajeError = "Ingrese usuario y clave.";
                return;
            }

            var usuario = await _usuarioService.LoginAsync(new LoginInputDto
            {
                NombreUsuario = NombreUsuario,
                Clave = Clave,
                ReturnUrl = "/"
            });

            if (usuario is null)
            {
                MensajeError = "Usuario o clave incorrectos.";
                return;
            }

            _authStateService.SetUsuario(usuario);
            Clave = string.Empty;
            LoginSucceeded?.Invoke(usuario);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
