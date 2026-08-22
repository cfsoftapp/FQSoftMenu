using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using Menu.Enums;
using Menu.Security;
using Menu.Services;
using Menu.Services.Cierres;

namespace Menu.Desktop.ViewModels;

public sealed class CierresViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private readonly ICierreService _cierreService;
    private readonly AuthStateService _authState;
    private readonly AsyncRelayCommand _actualizarCommand;
    private readonly RelayCommand _nuevoCierreCommand;
    private bool _isBusy;
    private string _estado = "Consulta y administra los cierres de facturacion.";
    private string _estadoBackground = "#EEF6FF";
    private string _estadoBorderBrush = "#90CAF9";
    private string _estadoForeground = "#1565C0";

    public CierresViewModel(ICierreService cierreService, AuthStateService authState)
    {
        _cierreService = cierreService;
        _authState = authState;
        _actualizarCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        _nuevoCierreCommand = new RelayCommand(
            () => RequestCierreDialog?.Invoke(null),
            () => CanManage);
    }

    public event Action<CierreProveedorRowViewModel?>? RequestCierreDialog;

    public ObservableCollection<CierreProveedorRowViewModel> Cierres { get; } = new();

    public ICommand ActualizarCommand => _actualizarCommand;

    public ICommand NuevoCierreCommand => _nuevoCierreCommand;

    public bool CanManage => _authState.TienePermiso(Permisos.CierresGestionar);

    public int TotalCierres => Cierres.Count;

    public int PendientesConfirmar => Cierres.Count(x => x.EsBorrador);

    public string MontoConfirmadoText => Cierres
        .Where(x => !x.EsBorrador)
        .Sum(x => x.Cierre.TotalLiquidarProveedor)
        .ToString("C2", Culture);

    public string Estado
    {
        get => _estado;
        private set => SetProperty(ref _estado, value);
    }

    public string EstadoBackground
    {
        get => _estadoBackground;
        private set => SetProperty(ref _estadoBackground, value);
    }

    public string EstadoBorderBrush
    {
        get => _estadoBorderBrush;
        private set => SetProperty(ref _estadoBorderBrush, value);
    }

    public string EstadoForeground
    {
        get => _estadoForeground;
        private set => SetProperty(ref _estadoForeground, value);
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            OnPropertyChanged(nameof(CanManage));
            _nuevoCierreCommand.RaiseCanExecuteChanged();

            var cierres = await _cierreService.ObtenerCierresProveedorAsync();
            Cierres.Clear();
            foreach (var cierre in cierres)
                Cierres.Add(new CierreProveedorRowViewModel(cierre));

            NotifySummary();
            SetEstado(
                cierres.Count == 0
                    ? "Todavia no hay cierres de facturacion."
                    : $"Se encontraron {cierres.Count} cierres de facturacion.",
                cierres.Count > 0);
        }
        catch (Exception ex)
        {
            SetEstado($"No se pudieron cargar los cierres: {ex.Message}", false);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Open(CierreProveedorRowViewModel cierre) =>
        RequestCierreDialog?.Invoke(cierre);

    public CierreProveedorDialogViewModel CreateDialogViewModel() =>
        new(_cierreService, _authState);

    public async Task<bool> DeleteAsync(CierreProveedorRowViewModel cierre)
    {
        if (!CanManage)
        {
            SetEstado("No tiene permiso para gestionar cierres de facturacion.", false);
            return false;
        }

        var result = await _cierreService.EliminarBorradorProveedorAsync(cierre.Id);
        SetEstado(result.Message, result.Success);
        if (result.Success)
            await LoadAsync();
        return result.Success;
    }

    public Task<byte[]> GenerateExcelAsync(CierreProveedorRowViewModel cierre) =>
        _cierreService.GenerarExcelProveedorAsync(cierre.Id);

    private bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
                _actualizarCommand.RaiseCanExecuteChanged();
        }
    }

    private void NotifySummary()
    {
        OnPropertyChanged(nameof(TotalCierres));
        OnPropertyChanged(nameof(PendientesConfirmar));
        OnPropertyChanged(nameof(MontoConfirmadoText));
    }

    private void SetEstado(string message, bool success)
    {
        Estado = message;
        (EstadoBackground, EstadoBorderBrush, EstadoForeground) = success
            ? ("#E8F5E9", "#A5D6A7", "#2E7D32")
            : ("#FFF8E1", "#FFCC80", "#E65100");
    }
}
