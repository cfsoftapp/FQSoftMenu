using System.Collections.ObjectModel;
using System.Globalization;
using Menu.DTOs;
using Menu.DTOs.Cierres;
using Menu.Security;
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
    private bool _isExisting;
    private bool _forceReadOnly;
    private bool _mostrarActivos;
    private bool _hasUnsavedChanges;
    private bool _isApplyingBorrador;

    public CierreProveedorDialogViewModel(ICierreService cierreService, AuthStateService authState)
    {
        _cierreService = cierreService;
        _authState = authState;
        ToggleActivosCommand = new RelayCommand(() => MostrarActivos = !MostrarActivos);
    }

    public ObservableCollection<CierreProveedorItemRowViewModel> Items { get; } = new();

    public ObservableCollection<CierreProveedorGrupoViewModel> GruposActivos { get; } = new();

    public ObservableCollection<CierreProveedorGrupoViewModel> GruposPlanilla { get; } = new();

    public RelayCommand ToggleActivosCommand { get; }

    public DateTime FechaDesde
    {
        get => _fechaDesde;
        set
        {
            if (SetProperty(ref _fechaDesde, value.Date))
                InvalidateCalculatedDraft();
        }
    }

    public DateTime FechaHasta
    {
        get => _fechaHasta;
        set
        {
            if (SetProperty(ref _fechaHasta, value.Date))
                InvalidateCalculatedDraft();
        }
    }

    public string Observacion
    {
        get => _observacion;
        set
        {
            if (SetProperty(ref _observacion, value) &&
                !_isApplyingBorrador &&
                TieneBorrador &&
                CanEdit)
            {
                HasUnsavedChanges = true;
                _guardadoParaConfirmar = false;
                NotifyState();
            }
        }
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

    public bool MostrarActivos
    {
        get => _mostrarActivos;
        set
        {
            if (SetProperty(ref _mostrarActivos, value))
                OnPropertyChanged(nameof(ToggleActivosText));
        }
    }

    public string ToggleActivosText => MostrarActivos ? "OCULTAR" : "REVISAR ACTIVOS";

    public bool HasGruposActivos => GruposActivos.Count > 0;

    public bool HasGruposPlanilla => GruposPlanilla.Count > 0;

    public bool TieneBorrador =>
        _borrador is not null &&
        (Items.Count > 0 || _borrador.CierreProveedorId.HasValue);

    public bool ShowInitialPrompt => !TieneBorrador;

    public bool YaConfirmado => _borrador?.YaConfirmado == true;

    public bool CanManage => _authState.TienePermiso(Permisos.CierresGestionar);

    public bool CanEdit => CanManage && TieneBorrador && Items.Count > 0 && !YaConfirmado && !_forceReadOnly;

    public bool CanCalculate => CanManage && !_isExisting && !YaConfirmado && !_forceReadOnly;

    public bool CanChangePeriod => CanManage && !_isExisting && !YaConfirmado && !_forceReadOnly;

    public bool CanConfirm => CanEdit && _guardadoParaConfirmar && Items.Count > 0;

    public bool CanExport => _borrador?.CierreProveedorId.HasValue == true;

    public bool ShowCalculate => CanCalculate;

    public bool ShowEditActions => CanEdit;

    public bool ShowExport => CanExport;

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public string DialogTitle => TieneBorrador
        ? YaConfirmado || _forceReadOnly || !CanManage ? "Detalle del cierre de facturacion" : "Revisar cierre de facturacion"
        : "Nuevo cierre de facturacion";

    public int TotalMenusActivos => _borrador?.TotalMenusActivos ?? 0;

    public int TotalAdicionalesEmpresa => _borrador?.TotalAdicionalesEmpresa ?? 0;

    public int TotalMenusPlanilla => _borrador?.TotalMenusPlanilla ?? 0;

    public int TotalExcepciones => _borrador?.TotalMenusPlanillaExcluidos ?? 0;

    public string TotalActivosText => (_borrador?.TotalPersonalActivo ?? 0).ToString("C2", Culture);

    public string TotalPlanillaText => (_borrador?.TotalPlanilla ?? 0).ToString("C2", Culture);

    public string TotalRevisionText => (_borrador?.TotalExcluidoRevision ?? 0).ToString("C2", Culture);

    public string TotalFacturarText => (_borrador?.TotalLiquidarProveedor ?? 0).ToString("C2", Culture);

    public async Task LoadExistingAsync(CierreProveedorRowViewModel cierre, bool readOnly = false)
    {
        _isExisting = true;
        _forceReadOnly = readOnly;
        ApplyBorrador(await _cierreService.ObtenerCierreProveedorAsync(cierre.Id));
        _guardadoParaConfirmar = !YaConfirmado;
        Mensaje = YaConfirmado
            ? "Este cierre ya esta confirmado y solo puede consultarse."
            : readOnly
                ? "Vista de consulta del borrador. Usa la accion Editar para realizar cambios."
                : "Borrador cargado. Revisa las excepciones antes de guardar o confirmar.";
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
            ApplyBorrador(await _cierreService.GenerarBorradorProveedorAsync(new CierreFiltroDto
            {
                FechaDesde = FechaDesde,
                FechaHasta = FechaHasta
            }));

            _guardadoParaConfirmar =
                _borrador?.CierreProveedorId.HasValue == true &&
                !YaConfirmado;
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
            HasUnsavedChanges = false;
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
        {
            await ReloadAsync();
            HasUnsavedChanges = false;
        }
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

        if (!CanManage)
            return ResultadoOperacionDto.Fail("No tiene permiso para gestionar cierres de facturacion.");

        if (Items.Any(x => x.Excluir && string.IsNullOrWhiteSpace(x.Motivo)))
            return ResultadoOperacionDto.Fail("Indica un motivo para cada item excluido.");

        return null;
    }

    private ConfirmarCierreProveedorDto CreateInput() => new()
    {
        CierreProveedorId = _borrador?.CierreProveedorId,
        FechaDesde = FechaDesde,
        FechaHasta = FechaHasta,
        Items = Items.Select(x => x.Item).ToList(),
        Observacion = string.IsNullOrWhiteSpace(Observacion) ? null : Observacion.Trim(),
        UsuarioConfirmacionId = _authState.UsuarioActual?.Id ?? 0,
        UsuarioConfirmacionNombre = _authState.UsuarioActual?.NombreCompleto ?? string.Empty
    };

    private async Task ReloadAsync()
    {
        if (_borrador?.CierreProveedorId is int cierreId)
        {
            ApplyBorrador(await _cierreService.ObtenerCierreProveedorAsync(cierreId));
            return;
        }

        ApplyBorrador(await _cierreService.GenerarBorradorProveedorAsync(new CierreFiltroDto
        {
            FechaDesde = FechaDesde,
            FechaHasta = FechaHasta
        }));
    }

    private void ApplyBorrador(CierreProveedorBorradorDto borrador)
    {
        _isApplyingBorrador = true;
        _borrador = borrador;
        _isExisting = borrador.CierreProveedorId.HasValue;
        FechaDesde = borrador.FechaDesde;
        FechaHasta = borrador.FechaHasta;
        Observacion = borrador.Observacion ?? string.Empty;
        Items.Clear();
        foreach (var item in borrador.Items)
            Items.Add(new CierreProveedorItemRowViewModel(item, NotifyTotals));
        RebuildGroups();
        HasUnsavedChanges = false;
        _isApplyingBorrador = false;
    }

    private void InvalidateCalculatedDraft()
    {
        if (_isApplyingBorrador || _isExisting || _borrador is null)
            return;

        _borrador = null;
        Items.Clear();
        GruposActivos.Clear();
        GruposPlanilla.Clear();
        _guardadoParaConfirmar = false;
        HasUnsavedChanges = false;
        Mensaje = "El periodo cambio. Presiona Calcular para actualizar el borrador.";
        NotifyState();
    }

    private void NotifyTotals()
    {
        if (_borrador is not null)
        {
            _borrador.Items = Items.Select(x => x.Item).ToList();
            _guardadoParaConfirmar = false;
            HasUnsavedChanges = true;
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

        OnPropertyChanged(nameof(HasGruposActivos));
        OnPropertyChanged(nameof(HasGruposPlanilla));
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
        OnPropertyChanged(nameof(ShowInitialPrompt));
        OnPropertyChanged(nameof(YaConfirmado));
        OnPropertyChanged(nameof(CanEdit));
        OnPropertyChanged(nameof(CanCalculate));
        OnPropertyChanged(nameof(CanChangePeriod));
        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(ShowCalculate));
        OnPropertyChanged(nameof(ShowEditActions));
        OnPropertyChanged(nameof(ShowExport));
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
