using Menu.DTOs.Cierres;
using Menu.Enums;
using Menu.Security;
using Menu.Services.Cierres;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace Menu.Components.Pages.Cierres;

public partial class Cierres : ComponentBase
{
    [Inject] private ICierreService CierreService { get; set; } = default!;

    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private CierreFiltroDto _filtro = new();
    private CierreResumenDto _resumen = new();
    private List<CierreDto> _cierres = new();
    private List<CierreProveedorListadoDto> _historialProveedor = new();
    private CierreProveedorBorradorDto? _borradorProveedor;
    private readonly System.Globalization.CultureInfo _culture = new("es-PE");
    private string? _observacionConfirmacion;
    private string _buscarEmpleadoCierre = string.Empty;
    private int? _empleadoExpandidoId;
    private bool _mostrarNuevoCierre;
    private bool _mostrarActivos;
    private bool _cargando;
    private bool _cargandoBorrador;
    private bool _guardandoBorrador;
    private bool _confirmandoProveedor;
    private bool _borradorGuardadoParaConfirmar;
    private bool _cierreExistente;
    private bool _modoConsulta;

    private int TotalCierresProveedor => _historialProveedor.Count;

    private int TotalPendientesConfirmar => _historialProveedor.Count(x => x.Estado == EstadoCierreProveedor.Borrador);

    private decimal TotalMontoConfirmado => _historialProveedor
        .Where(x => x.Estado == EstadoCierreProveedor.Confirmado)
        .Sum(x => x.TotalLiquidarProveedor);

    private IEnumerable<GrupoEmpleadoProveedor> GruposPlanilla => FiltrarGrupos(false);

    private IEnumerable<GrupoEmpleadoProveedor> GruposActivos => FiltrarGrupos(true);

    protected override async Task OnInitializedAsync()
    {
        _filtro.FechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _filtro.FechaHasta = DateTime.Today;

        await CargarPantallaAsync();
    }

    private async Task CargarPantallaAsync()
    {
        if (_cargando)
            return;

        try
        {
            _cargando = true;
            _historialProveedor = await CierreService.ObtenerCierresProveedorAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar cierres de facturación: {ex.Message}", Severity.Error);
            _historialProveedor = new List<CierreProveedorListadoDto>();
        }
        finally
        {
            _cargando = false;
        }
    }

    private async Task BuscarAsync()
    {
        if (_cargando)
            return;

        if (_filtro.FechaDesde is null || _filtro.FechaHasta is null)
        {
            Snackbar.Add("Debe seleccionar fecha desde y fecha hasta.", Severity.Warning);
            return;
        }

        if (_filtro.FechaDesde.Value.Date > _filtro.FechaHasta.Value.Date)
        {
            Snackbar.Add("La fecha desde no puede ser mayor que la fecha hasta.", Severity.Warning);
            return;
        }

        try
        {
            _cargando = true;
            _resumen = await CierreService.ObtenerResumenAsync(_filtro);
            _cierres = await CierreService.ObtenerCierresAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar cierres: {ex.Message}", Severity.Error);
            _resumen = new CierreResumenDto();
            _cierres = new List<CierreDto>();
        }
        finally
        {
            _cargando = false;
        }
    }

    private void AbrirNuevoCierre()
    {
        if (!PuedeGestionarCierres())
            return;

        _filtro.FechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _filtro.FechaHasta = DateTime.Today;
        _borradorProveedor = null;
        _observacionConfirmacion = string.Empty;
        _buscarEmpleadoCierre = string.Empty;
        _empleadoExpandidoId = null;
        _mostrarActivos = false;
        _borradorGuardadoParaConfirmar = false;
        _cierreExistente = false;
        _modoConsulta = false;
        _mostrarNuevoCierre = true;
    }

    private void CerrarNuevoCierre()
    {
        _mostrarNuevoCierre = false;
        _borradorProveedor = null;
        _empleadoExpandidoId = null;
        _borradorGuardadoParaConfirmar = false;
        _cierreExistente = false;
        _modoConsulta = false;
    }

    private async Task AbrirCierreProveedorAsync(
        CierreProveedorListadoDto cierre,
        bool soloConsulta)
    {
        if (!soloConsulta && !PuedeGestionarCierres())
            return;

        try
        {
            _cargandoBorrador = true;
            _borradorProveedor = await CierreService.ObtenerCierreProveedorAsync(cierre.Id);
            _filtro.FechaDesde = _borradorProveedor.FechaDesde;
            _filtro.FechaHasta = _borradorProveedor.FechaHasta;
            _observacionConfirmacion = _borradorProveedor.Observacion;
            _buscarEmpleadoCierre = string.Empty;
            _empleadoExpandidoId = null;
            _mostrarActivos = false;
            _borradorGuardadoParaConfirmar = !_borradorProveedor.YaConfirmado;
            _cierreExistente = true;
            _modoConsulta = soloConsulta || _borradorProveedor.YaConfirmado;
            _mostrarNuevoCierre = true;
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al abrir cierre: {ex.Message}", Severity.Error);
            _borradorProveedor = null;
        }
        finally
        {
            _cargandoBorrador = false;
        }
    }

    private async Task ConfirmarEliminarBorradorAsync(CierreProveedorListadoDto cierre)
    {
        if (!PuedeGestionarCierres())
            return;

        if (cierre.Estado != EstadoCierreProveedor.Borrador)
        {
            Snackbar.Add("Solo se pueden eliminar cierres en borrador.", Severity.Warning);
            return;
        }

        var confirmado = await JsRuntime.InvokeAsync<bool>(
            "confirm",
            "Esto eliminará el borrador y su detalle. No afectará los consumos registrados.");

        if (!confirmado)
            return;

        try
        {
            var result = await CierreService.EliminarBorradorProveedorAsync(cierre.Id);

            if (result.Success)
            {
                Snackbar.Add(result.Message, Severity.Success);

                if (_borradorProveedor?.CierreProveedorId == cierre.Id)
                    CerrarNuevoCierre();

                await CargarPantallaAsync();
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al eliminar el borrador: {ex.Message}", Severity.Error);
        }
    }

    private async Task GenerarBorradorProveedorAsync()
    {
        if (!PuedeGestionarCierres())
            return;

        if (_filtro.FechaDesde is null || _filtro.FechaHasta is null)
        {
            Snackbar.Add("Debe seleccionar fecha desde y fecha hasta.", Severity.Warning);
            return;
        }

        if (_filtro.FechaDesde.Value.Date > _filtro.FechaHasta.Value.Date)
        {
            Snackbar.Add("La fecha desde no puede ser mayor que la fecha hasta.", Severity.Warning);
            return;
        }

        try
        {
            _cargandoBorrador = true;
            _borradorProveedor = await CierreService.GenerarBorradorProveedorAsync(_filtro);
            _observacionConfirmacion = _borradorProveedor.Observacion;
            _empleadoExpandidoId = null;
            _borradorGuardadoParaConfirmar =
                _borradorProveedor.CierreProveedorId.HasValue &&
                !_borradorProveedor.YaConfirmado;

            if (!_borradorProveedor.Items.Any())
            Snackbar.Add("No hay consumos de empresa cliente o planilla para facturar.", Severity.Info);
            else if (_borradorProveedor.YaConfirmado)
                Snackbar.Add("Se recupero una liquidacion ya confirmada para este rango.", Severity.Info);
            else
                Snackbar.Add(_borradorGuardadoParaConfirmar
                    ? "Borrador recuperado. Puedes revisarlo o confirmarlo."
                    : "Borrador calculado. Guarda el borrador para habilitar la confirmacion.", Severity.Success);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al generar borrador: {ex.Message}", Severity.Error);
            _borradorProveedor = null;
        }
        finally
        {
            _cargandoBorrador = false;
        }
    }

    private async Task GuardarBorradorProveedorAsync()
    {
        if (!PuedeGestionarCierres())
            return;

        if (_borradorProveedor is null)
        {
            Snackbar.Add("Primero genere el borrador de liquidacion.", Severity.Warning);
            return;
        }

        if (!AuthState.EstaAutenticado || AuthState.UsuarioActual is null)
        {
            Snackbar.Add("Debe iniciar sesion para guardar el borrador.", Severity.Warning);
            Navigation.NavigateTo("/login");
            return;
        }

        if (!ValidarMotivosExcepciones())
            return;

        try
        {
            _guardandoBorrador = true;

            var input = CrearInputProveedor();
            var result = await CierreService.GuardarBorradorProveedorAsync(input);

            if (result.Success)
            {
                Snackbar.Add(result.Message, Severity.Success);
                _borradorProveedor = await CierreService.GenerarBorradorProveedorAsync(_filtro);
                _borradorGuardadoParaConfirmar = !_borradorProveedor.YaConfirmado;
                _cierreExistente = _borradorProveedor.CierreProveedorId.HasValue;
                _modoConsulta = _borradorProveedor.YaConfirmado;
                _observacionConfirmacion = _borradorProveedor.Observacion;
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al guardar borrador: {ex.Message}", Severity.Error);
        }
        finally
        {
            _guardandoBorrador = false;
        }
    }

    private void CambiarExcepcionPlanilla(CierreProveedorItemDto item, bool value)
    {
        if (_modoConsulta || _borradorProveedor?.YaConfirmado == true)
            return;

        item.ExcluirDeProveedor = value;

        if (!value)
            item.MotivoExclusion = null;
    }

    private async Task ConfirmarLiquidacionProveedorAsync()
    {
        if (!PuedeGestionarCierres())
            return;

        if (_borradorProveedor is null)
        {
            Snackbar.Add("Primero genere el borrador de liquidacion.", Severity.Warning);
            return;
        }

        if (!AuthState.EstaAutenticado || AuthState.UsuarioActual is null)
        {
            Snackbar.Add("Debe iniciar sesion para confirmar el cierre.", Severity.Warning);
            Navigation.NavigateTo("/login");
            return;
        }

        if (!ValidarMotivosExcepciones())
            return;

        var confirmado = await JsRuntime.InvokeAsync<bool>(
            "confirm",
            "¿Confirmar este cierre de facturación? Después de confirmar no podrá modificarse.");

        if (!confirmado)
            return;

        try
        {
            _confirmandoProveedor = true;

            var input = CrearInputProveedor();
            var result = await CierreService.ConfirmarLiquidacionProveedorAsync(input);

            if (result.Success)
            {
                Snackbar.Add(result.Message, Severity.Success);
                _borradorProveedor = await CierreService.GenerarBorradorProveedorAsync(_filtro);
                _borradorGuardadoParaConfirmar = false;
                _cierreExistente = _borradorProveedor.CierreProveedorId.HasValue;
                _modoConsulta = true;
                _observacionConfirmacion = _borradorProveedor.Observacion;
            }
            else
            {
                Snackbar.Add(result.Message, Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al confirmar liquidacion: {ex.Message}", Severity.Error);
        }
        finally
        {
            _confirmandoProveedor = false;
        }
    }

    private bool ValidarMotivosExcepciones()
    {
        if (_borradorProveedor is null)
            return false;

        var excepcionSinMotivo = _borradorProveedor.Items.Any(x =>
            x.ExcluirDeProveedor &&
            string.IsNullOrWhiteSpace(x.MotivoExclusion));

        if (!excepcionSinMotivo)
            return true;

        Snackbar.Add("Ingrese motivo para cada excepcion de planilla.", Severity.Warning);
        return false;
    }

    private bool PuedeGestionarCierres()
    {
        if (AuthState.TienePermiso(Permisos.CierresGestionar))
            return true;

        Snackbar.Add("No tiene permiso para gestionar cierres de facturacion.", Severity.Warning);
        return false;
    }

    private ConfirmarCierreProveedorDto CrearInputProveedor()
    {
        return new ConfirmarCierreProveedorDto
        {
            CierreProveedorId = _borradorProveedor!.CierreProveedorId,
            FechaDesde = _borradorProveedor!.FechaDesde,
            FechaHasta = _borradorProveedor.FechaHasta,
            Items = _borradorProveedor.Items,
            Observacion = _observacionConfirmacion,
            UsuarioConfirmacionId = AuthState.UsuarioActual?.Id ?? 0,
            UsuarioConfirmacionNombre = AuthState.UsuarioActual?.NombreCompleto ?? string.Empty
        };
    }

    private async Task ConfirmarYActualizarAsync()
    {
        await ConfirmarLiquidacionProveedorAsync();
        await CargarPantallaAsync();
    }

    private async Task GuardarYActualizarAsync()
    {
        await GuardarBorradorProveedorAsync();
        await CargarPantallaAsync();
    }

    private IEnumerable<GrupoEmpleadoProveedor> FiltrarGrupos(bool activos)
    {
        if (_borradorProveedor is null)
            return Enumerable.Empty<GrupoEmpleadoProveedor>();

        var grupos = _borradorProveedor.Items
            .Where(x => activos
                ? x.TipoPagoMenu == TipoPagoMenu.Empresa
                : x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
            .GroupBy(x => new { x.EmpleadoId, x.Dni, x.EmpleadoNombre })
            .Select(x => new GrupoEmpleadoProveedor
            {
                EmpleadoId = x.Key.EmpleadoId,
                Dni = x.Key.Dni,
                EmpleadoNombre = x.Key.EmpleadoNombre,
                Items = x.OrderBy(i => i.Fecha).ToList()
            });

        if (!string.IsNullOrWhiteSpace(_buscarEmpleadoCierre))
        {
            var texto = _buscarEmpleadoCierre.Trim().ToLower();
            grupos = grupos.Where(x =>
                x.Dni.ToLower().Contains(texto) ||
                x.EmpleadoNombre.ToLower().Contains(texto));
        }

        return grupos.OrderBy(x => x.EmpleadoNombre).ToList();
    }

    private void ToggleEmpleado(int empleadoId)
    {
        _empleadoExpandidoId = _empleadoExpandidoId == empleadoId ? null : empleadoId;
    }

    private string GetExpandIcon(int empleadoId)
    {
        return _empleadoExpandidoId == empleadoId
            ? Icons.Material.Filled.KeyboardArrowUp
            : Icons.Material.Filled.KeyboardArrowDown;
    }

    private static Color GetEstadoColor(EstadoCierreProveedor estado)
    {
        return estado == EstadoCierreProveedor.Confirmado ? Color.Success : Color.Warning;
    }

    private sealed class GrupoEmpleadoProveedor
    {
        public int EmpleadoId { get; set; }

        public string Dni { get; set; } = string.Empty;

        public string EmpleadoNombre { get; set; } = string.Empty;

        public List<CierreProveedorItemDto> Items { get; set; } = new();

        public int CantidadItems => Items.Count;

        public int CantidadExcepciones => Items.Count(x => x.ExcluirDeProveedor);

        public decimal TotalProveedor => Items
            .Where(x => !x.ExcluirDeProveedor)
            .Sum(x => x.Importe);

        public decimal TotalRevision => Items
            .Where(x => x.ExcluirDeProveedor)
            .Sum(x => x.Importe);
    }
}
