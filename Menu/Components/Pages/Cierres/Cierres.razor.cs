using Menu.DTOs.Cierres;
using Menu.Enums;
using Menu.Services.Cierres;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Menu.Components.Pages.Cierres;

public partial class Cierres : ComponentBase
{
    [Inject] private ICierreService CierreService { get; set; } = default!;

    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private CierreFiltroDto _filtro = new();
    private CierreResumenDto _resumen = new();
    private List<CierreDto> _cierres = new();
    private CierreProveedorBorradorDto? _borradorProveedor;
    private readonly System.Globalization.CultureInfo _culture = new("es-PE");
    private string? _observacionConfirmacion;
    private bool _cargando;
    private bool _cargandoBorrador;
    private bool _confirmandoProveedor;

    protected override async Task OnInitializedAsync()
    {
        _filtro.FechaDesde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        _filtro.FechaHasta = DateTime.Today;

        await BuscarAsync();
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
            _borradorProveedor = null;
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

    private async Task GenerarBorradorProveedorAsync()
    {
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
            _observacionConfirmacion = string.Empty;
            _borradorProveedor = await CierreService.GenerarBorradorProveedorAsync(_filtro);

            if (!_borradorProveedor.Items.Any())
                Snackbar.Add("No hay menus de activo o planilla para liquidar con proveedor.", Severity.Info);
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

    private void CambiarExcepcionPlanilla(CierreProveedorItemDto item, bool value)
    {
        if (item.TipoPagoMenu != TipoPagoMenu.DescuentoPlanilla)
            return;

        item.ExcluirDeProveedor = value;

        if (!value)
            item.MotivoExclusion = null;
    }

    private async Task ConfirmarLiquidacionProveedorAsync()
    {
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

        var excepcionSinMotivo = _borradorProveedor.Items.Any(x =>
            x.ExcluirDeProveedor &&
            string.IsNullOrWhiteSpace(x.MotivoExclusion));

        if (excepcionSinMotivo)
        {
            Snackbar.Add("Ingrese motivo para cada excepcion de planilla.", Severity.Warning);
            return;
        }

        try
        {
            _confirmandoProveedor = true;

            var input = new ConfirmarCierreProveedorDto
            {
                FechaDesde = _borradorProveedor.FechaDesde,
                FechaHasta = _borradorProveedor.FechaHasta,
                Items = _borradorProveedor.Items,
                Observacion = _observacionConfirmacion,
                UsuarioConfirmacionId = AuthState.UsuarioActual.Id,
                UsuarioConfirmacionNombre = AuthState.UsuarioActual.NombreCompleto
            };

            var result = await CierreService.ConfirmarLiquidacionProveedorAsync(input);

            if (result.Success)
            {
                Snackbar.Add(result.Message, Severity.Success);
                _borradorProveedor = await CierreService.GenerarBorradorProveedorAsync(_filtro);
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
}
