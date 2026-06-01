using Menu.DTOs.Reportes;
using Menu.Services.Reportes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Menu.Components.Pages.Reportes;

public partial class Reportes : ComponentBase
{
    [Inject] private IReporteService ReporteService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private ReporteFiltroDto _filtro = new();
    private ReporteResumenDto _resumen = new();
    private List<ReporteEmpleadoDto> _empleados = new();

    private bool _cargando;

    protected override async Task OnInitializedAsync()
    {
        _filtro.FechaDesde = DateTime.Today;
        _filtro.FechaHasta = DateTime.Today;

        await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        if (_cargando)
            return;

        try
        {
            _cargando = true;

            _resumen = await ReporteService.ObtenerResumenAsync(_filtro);
            _empleados = await ReporteService.ObtenerDetalleEmpleadosAsync(_filtro);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error al cargar reportes: {ex.Message}", Severity.Error);

            _resumen = new ReporteResumenDto();
            _empleados = new List<ReporteEmpleadoDto>();
        }
        finally
        {
            _cargando = false;
        }
    }
}