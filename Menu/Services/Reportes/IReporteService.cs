using Menu.DTOs.Reportes;

namespace Menu.Services.Reportes;

public interface IReporteService
{
    Task<ReporteResumenDto> ObtenerResumenAsync(ReporteFiltroDto filtro);

    Task<List<ReporteEmpleadoDto>> ObtenerDetalleEmpleadosAsync(ReporteFiltroDto filtro);
}