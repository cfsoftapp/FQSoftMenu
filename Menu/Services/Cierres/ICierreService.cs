using Menu.DTOs.Cierres;
using Menu.DTOs;

namespace Menu.Services.Cierres;

public interface ICierreService
{
    Task<CierreResumenDto> ObtenerResumenAsync(CierreFiltroDto filtro);

    Task<List<CierreDto>> ObtenerCierresAsync(CierreFiltroDto filtro);

    Task<CierreProveedorBorradorDto> GenerarBorradorProveedorAsync(CierreFiltroDto filtro);

    Task<ResultadoOperacionDto> ConfirmarLiquidacionProveedorAsync(ConfirmarCierreProveedorDto input);
}
