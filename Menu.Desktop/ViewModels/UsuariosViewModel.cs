using System.Collections.ObjectModel;
using System.Windows.Input;
using Menu.Models;
using Menu.Security;
using Menu.Services;

namespace Menu.Desktop.ViewModels;

public sealed class UsuariosViewModel : ObservableObject
{
    private readonly UsuarioService _usuarioService;
    private readonly AuthStateService _authState;
    private readonly AsyncRelayCommand _refreshCommand;
    private readonly AsyncRelayCommand _saveCommand;
    private readonly AsyncRelayCommand _savePasswordCommand;
    private readonly RelayCommand<UsuarioSistema> _editCommand;
    private readonly RelayCommand<UsuarioSistema> _toggleCommand;
    private readonly RelayCommand<UsuarioSistema> _changePasswordCommand;
    private bool _isBusy;
    private bool _showForm;
    private bool _showPasswordForm;
    private int _editingId;
    private int _passwordUserId;
    private string _nombreUsuario = string.Empty;
    private string _nombreCompleto = string.Empty;
    private string _clave = string.Empty;
    private string _nuevaClave = string.Empty;
    private int? _rolSistemaId;
    private bool _activo = true;
    private string _passwordUserName = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _statusIsError;

    public UsuariosViewModel(UsuarioService usuarioService, AuthStateService authState)
    {
        _usuarioService = usuarioService;
        _authState = authState;
        _refreshCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy && CanView);
        _saveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy && CanEdit);
        _savePasswordCommand = new AsyncRelayCommand(SavePasswordAsync, () => !IsBusy && CanEdit);
        _editCommand = new RelayCommand<UsuarioSistema>(Edit, x => x is not null && CanEdit);
        _toggleCommand = new RelayCommand<UsuarioSistema>(async x => await ToggleAsync(x), x => x is not null && CanEdit);
        _changePasswordCommand = new RelayCommand<UsuarioSistema>(StartPasswordChange, x => x is not null && CanEdit);
        NewCommand = new RelayCommand(New, () => CanCreate);
        CancelCommand = new RelayCommand(Cancel);
        CancelPasswordCommand = new RelayCommand(CancelPassword);
    }

    public ObservableCollection<UsuarioSistema> Usuarios { get; } = new();
    public ObservableCollection<RolSistema> Roles { get; } = new();

    public ICommand RefreshCommand => _refreshCommand;
    public ICommand SaveCommand => _saveCommand;
    public ICommand SavePasswordCommand => _savePasswordCommand;
    public ICommand EditCommand => _editCommand;
    public ICommand ToggleCommand => _toggleCommand;
    public ICommand ChangePasswordCommand => _changePasswordCommand;
    public RelayCommand NewCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand CancelPasswordCommand { get; }

    public bool CanView => _authState.TienePermiso(Permisos.UsuariosVer);
    public bool CanCreate => _authState.TienePermiso(Permisos.UsuariosCrear);
    public bool CanEdit => _authState.TienePermiso(Permisos.UsuariosEditar);
    public string FormTitle => EditingId == 0 ? "Nuevo usuario" : "Editar usuario";
    public string StatusColor => StatusIsError ? "#C62828" : "#087F5B";

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            RaiseCommandStates();
        }
    }

    public bool ShowForm
    {
        get => _showForm;
        private set => SetProperty(ref _showForm, value);
    }

    public bool ShowPasswordForm
    {
        get => _showPasswordForm;
        private set => SetProperty(ref _showPasswordForm, value);
    }

    public int EditingId
    {
        get => _editingId;
        private set
        {
            if (SetProperty(ref _editingId, value))
                OnPropertyChanged(nameof(FormTitle));
        }
    }

    public string NombreUsuario { get => _nombreUsuario; set => SetProperty(ref _nombreUsuario, value); }
    public string NombreCompleto { get => _nombreCompleto; set => SetProperty(ref _nombreCompleto, value); }
    public string Clave { get => _clave; set => SetProperty(ref _clave, value); }
    public string NuevaClave { get => _nuevaClave; set => SetProperty(ref _nuevaClave, value); }
    public int? RolSistemaId { get => _rolSistemaId; set => SetProperty(ref _rolSistemaId, value); }
    public bool Activo { get => _activo; set => SetProperty(ref _activo, value); }
    public string PasswordUserName { get => _passwordUserName; private set => SetProperty(ref _passwordUserName, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public bool StatusIsError
    {
        get => _statusIsError;
        private set
        {
            if (SetProperty(ref _statusIsError, value))
                OnPropertyChanged(nameof(StatusColor));
        }
    }

    public async Task LoadAsync()
    {
        if (!CanView)
            return;

        IsBusy = true;
        SetStatus("Cargando usuarios...", false);

        try
        {
            var usuarios = await _usuarioService.GetAllAsync();
            var roles = await _usuarioService.GetRolesActivosAsync();

            Usuarios.Clear();
            foreach (var usuario in usuarios)
                Usuarios.Add(usuario);

            Roles.Clear();
            foreach (var rol in roles)
                Roles.Add(rol);

            SetStatus($"{Usuarios.Count} usuario(s) registrado(s).", false);
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudieron cargar los usuarios: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void New()
    {
        EditingId = 0;
        NombreUsuario = string.Empty;
        NombreCompleto = string.Empty;
        Clave = string.Empty;
        RolSistemaId = Roles.FirstOrDefault()?.Id;
        Activo = true;
        ShowPasswordForm = false;
        ShowForm = true;
        SetStatus("Complete los datos del nuevo usuario.", false);
    }

    private void Edit(UsuarioSistema? usuario)
    {
        if (usuario is null)
            return;

        EditingId = usuario.Id;
        NombreUsuario = usuario.NombreUsuario;
        NombreCompleto = usuario.NombreCompleto;
        Clave = string.Empty;
        RolSistemaId = usuario.RolSistemaId;
        Activo = usuario.Activo;
        ShowPasswordForm = false;
        ShowForm = true;
    }

    private async Task SaveAsync()
    {
        IsBusy = true;

        try
        {
            (bool Success, string Message) result;

            if (EditingId == 0)
            {
                result = await _usuarioService.CrearUsuarioAsync(
                    NombreUsuario,
                    NombreCompleto,
                    Clave,
                    RolSistemaId ?? 0);
            }
            else
            {
                result = await _usuarioService.ActualizarUsuarioAsync(new UsuarioSistema
                {
                    Id = EditingId,
                    NombreUsuario = NombreUsuario,
                    NombreCompleto = NombreCompleto,
                    RolSistemaId = RolSistemaId ?? 0,
                    Activo = Activo
                });
            }

            SetStatus(result.Message, !result.Success);
            if (!result.Success)
                return;

            ShowForm = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"No se pudo guardar el usuario: {ex.Message}", true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ToggleAsync(UsuarioSistema? usuario)
    {
        if (usuario is null)
            return;

        IsBusy = true;
        try
        {
            var result = await _usuarioService.ToggleActivoAsync(usuario.Id);
            SetStatus(result.Message, !result.Success);
            if (result.Success)
                await LoadAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void StartPasswordChange(UsuarioSistema? usuario)
    {
        if (usuario is null)
            return;

        _passwordUserId = usuario.Id;
        PasswordUserName = usuario.NombreCompleto;
        NuevaClave = string.Empty;
        ShowForm = false;
        ShowPasswordForm = true;
    }

    private async Task SavePasswordAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _usuarioService.CambiarClaveAsync(_passwordUserId, NuevaClave);
            SetStatus(result.Message, !result.Success);
            if (result.Success)
                CancelPassword();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void Cancel()
    {
        ShowForm = false;
        Clave = string.Empty;
    }

    private void CancelPassword()
    {
        ShowPasswordForm = false;
        NuevaClave = string.Empty;
        _passwordUserId = 0;
        PasswordUserName = string.Empty;
    }

    private void SetStatus(string message, bool isError)
    {
        StatusMessage = message;
        StatusIsError = isError;
    }

    private void RaiseCommandStates()
    {
        _refreshCommand.RaiseCanExecuteChanged();
        _saveCommand.RaiseCanExecuteChanged();
        _savePasswordCommand.RaiseCanExecuteChanged();
        _editCommand.RaiseCanExecuteChanged();
        _toggleCommand.RaiseCanExecuteChanged();
        _changePasswordCommand.RaiseCanExecuteChanged();
        NewCommand.RaiseCanExecuteChanged();
    }
}
