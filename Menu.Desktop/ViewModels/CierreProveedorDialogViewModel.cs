using System.Collections.ObjectModel;
using System.Globalization;
using Menu.DTOs;
using Menu.DTOs.Cierres;
using Menu.Services;
using Menu.Services.Cierres;

namespace Menu.Desktop.ViewModels;

public sealed class CierreProveedorDialogViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private readonly ICierreService _cierreService;
    private readonly AuthStateService _authState;
    private DateTime _fechaDesde = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateTime _fechaHasta = DateTime.Today;
    private CierreProveedorBorradorDto? _borrador;
    private string _observacion = string.Empty;
    private string _search = string.Empty;
    private string _mensaje = "Selecciona un periodo y calcula el borrador.";
    private bool _guardadoParaConfirmar;

    public CierreProveedorDialogViewModel(ICierreService cierreService, AuthStateService authState)
    {
        _cierreService = cierreService;
        _authState = authState;
    }

    public ObservableCollection<CierreProveedorItemRowViewModel> Items { get; } = new();

    public ObservableCollection<CierreProveedorGrupoViewModel> GruposActivos { get; } = new();

    public ObservableCollection<CierreProveedorGrupoViewModel> GruposPlanilla { get; } = new();

    public DateTime FechaDesde
    {
        get => _fechaDesde;
        set => SetProperty(ref _fechaDesde, value.Date);
    }

    public DateTime FechaHasta
    {
        get => _fechaHasta;
        set => SetProperty(ref _fechaHasta, value.Date);
    }

    public string Observacion
    {
        get => _observacion;
        set => SetProperty(ref _observacion, value);
    }

    public string Search
    {
        get => _search;
        set
        {
            if (SetProperty(ref _search, value))
                RebuildGroups();
        }
    }

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public bool TieneBorrador => _borrador is not null;

    public bool YaConfirmado => _borrador?.YaConfirmado == true;

    public bool CanEdit => TieneBorrador && !YaConfirmado;

    public bool CanCalculate => !YaConfirmado;

    public bool CanConfirm => CanEdit && _guardadoParaConfirmar && Items.Count > 0;

    public bool CanExport => _borrador?.CierreProveedorId.HasValue == true;

    public string DialogTitle => TieneBorrador
        ? YaConfirmado ? "Detalle del cierre confirmado" : "Revisar cierre de facturacion"
        : "Nuevo cierre de facturacion";

    public int TotalMenusActivos => _borrador?.TotalMenusActivos ?? 0;

    public int TotalAdicionalesEmpresa => _borrador?.TotalAdicionalesEmpresa ?? 0;

    public int TotalMenusPlanilla => _borrador?.TotalMenusPlanilla ?? 0;

    public int TotalExcepciones => _borrador?.TotalMenusPlanillaExcluidos ?? 0;

    public string TotalActivosText => (_borrador?.TotalPersonalActivo ?? 0).ToString("C2", Culture);

    public string TotalPlanillaText => (_borrador?.TotalPlanilla ?? 0).ToString("C2", Culture);

    public string TotalRevisionText => (_borrador?.TotalExcluidoRevision ?? 0).ToString("C2", Culture);

    public string TotalFacturarText => (_borrador?.TotalLiquidarProveedor ?? 0).ToString("C2", Culture);

    public async Task LoadExistingAsync(CierreProveedorRowViewModel cierre)
    {
        FechaDesde = cierre.Cierre.FechaDesde;
        FechaHasta = cierre.Cierre.FechaHasta;
        await CalcularAsync();
        _guardadoParaConfirmar = !YaConfirmado;
        NotifyState();
    }

    public async Task CalcularAsync()
    {
        if (FechaDesde > FechaHasta)
        {
            Mensaje = "La fecha desde no puede ser mayor que la fecha hasta.";
            return;
        }

        try
        {
            _borrador = await _cierreService.GenerarBorradorProveedorAsync(new CierreFiltroDto
            {
                FechaDesde = FechaDesde,
                FechaHasta = FechaHasta
            });

            Items.Clear();
            foreach (var item in _borrador.Items)
                Items.Add(new CierreProveedorItemRowViewModel(item, NotifyTotals));
            RebuildGroups();

            _guardadoParaConfirmar = YaConfirmado;
            Mensaje = Items.Count == 0
                ? "No hay consumos de empresa cliente o planilla en este periodo."
                : YaConfirmado
                    ? "Este cierre ya esta confirmado y solo puede consultarse."
                    : "Borrador calculado. Revisa las excepciones y guarda antes de confirmar.";
            NotifyState();
        }
        catch (Exception ex)
        {
            _borrador = null;
            Items.Clear();
            GruposActivos.Clear();
            GruposPlanilla.Clear();
            Mensaje = $"No se pudo generar el borrador: {ex.Message}";
            NotifyState();
        }
    }

    public async Task<ResultadoOperacionDto> GuardarAsync()
    {
        var validation = Validate();
        if (validation is not null)
            return validation;

        var result = await _cierreService.GuardarBorradorProveedorAsync(CreateInput());
        Mensaje = result.Message;
        if (result.Success)
        {
            _guardadoParaConfirmar = true;
            await ReloadAsync();
        }
        NotifyState();
        return result;
    }

    public async Task<ResultadoOperacionDto> ConfirmarAsync()
    {
        var validation = Validate();
        if (validation is not null)
            return validation;

        if (!_guardadoParaConfirmar)
            return ResultadoOperacionDto.Fail("Guarda el borrador antes de confirmar.");

        var result = await _cierreService.ConfirmarLiquidacionProveedorAsync(CreateInput());
        Mensaje = result.Message;
        if (result.Success)
            await ReloadAsync();
        NotifyState();
        return result;
    }

    public Task<byte[]> GenerateExcelAsync()
    {
        if (_borrador?.CierreProveedorId is not int cierreId)
            throw new InvalidOperationException("Primero calcula o guarda el cierre.");

        return _cierreService.GenerarExcelProveedorAsync(cierreId);
    }

    private ResultadoOperacionDto? Validate()
    {
        if (_borrador is null)
            return ResultadoOperacionDto.Fail("Primero calcula el borrador.");

        if (!_authState.EstaAutenticado || _authState.UsuarioActual is null)
            return ResultadoOperacionDto.Fail("Debe iniciar sesion para guardar el cierre.");

        if (Items.Any(x => x.Excluir && string.IsNullOrWhiteSpace(x.Motivo)))
            return ResultadoOperacionDto.Fail("Indica un motivo para cada item excluido.");

        return null;
    }

    private ConfirmarCierreProveedorDto CreateInput() => new()
    {
        FechaDesde = FechaDesde,
        FechaHasta = FechaHasta,
        Items = Items.Select(x => x.Item).ToList(),
        Observacion = string.IsNullOrWhiteSpace(Observacion) ? null : Observacion.Trim(),
        UsuarioConfirmacionId = _authState.UsuarioActual?.Id ?? 0,
        UsuarioConfirmacionNombre = _authState.UsuarioActual?.NombreCompleto ?? string.Empty
    };

    private async Task ReloadAsync()
    {
        _borrador = await _cierreService.GenerarBorradorProveedorAsync(new CierreFiltroDto
        {
            FechaDesde = FechaDesde,
            FechaHasta = FechaHasta
        });
        Items.Clear();
        foreach (var item in _borrador.Items)
            Items.Add(new CierreProveedorItemRowViewModel(item, NotifyTotals));
        RebuildGroups();
    }

    private void NotifyTotals()
    {
        if (_borrador is not null)
        {
            _borrador.Items = Items.Select(x => x.Item).ToList();
            _guardadoParaConfirmar = false;
        }
        foreach (var grupo in GruposActivos.Concat(GruposPlanilla))
            grupo.NotifyTotals();
        NotifyState();
    }

    private void RebuildGroups()
    {
        var term = Search.Trim();
        var filtered = string.IsNullOrWhiteSpace(term)
            ? Items.AsEnumerable()
            : Items.Where(x =>
                x.Dni.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Comensal.Contains(term, StringComparison.OrdinalIgnoreCase));

        FillGroups(
            GruposActivos,
            filtered.Where(x => !x.Item.EsPlanilla));
        FillGroups(
            GruposPlanilla,
            filtered.Where(x => x.Item.EsPlanilla));
    }

    private static void FillGroups(
        ObservableCollection<CierreProveedorGrupoViewModel> target,
        IEnumerable<CierreProveedorItemRowViewModel> items)
    {
        target.Clear();
        foreach (var group in items
                     .GroupBy(x => new { x.Item.EmpleadoId, x.Dni, x.Comensal })
                     .OrderBy(x => x.Key.Comensal))
        {
            target.Add(new CierreProveedorGrupoViewModel(
                group.Key.EmpleadoId,
                group.Key.Dni,
                group.Key.Comensal,
                group));
        }
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(TieneBorrador));
        OnPropertyChanged(nameof(YaConfirmado));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanCalculate));
        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(DialogTitle));
        OnPropertyChanged(nameof(TotalMenusActivos));
        OnPropertyChanged(nameof(TotalAdicionalesEmpresa));
        OnPropertyChanged(nameof(TotalMenusPlanilla));
        OnPropertyChanged(nameof(TotalExcepciones));
        OnPropertyChanged(nameof(TotalActivosText));
        OnPropertyChanged(nameof(TotalPlanillaText));
        OnPropertyChanged(nameof(TotalRevisionText));
        OnPropertyChanged(nameof(TotalFacturarText));
    }
}
