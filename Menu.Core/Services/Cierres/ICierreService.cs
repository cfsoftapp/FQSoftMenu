using Menu.DTOs.Cierres;
using Menu.DTOs;

namespace Menu.Services.Cierres;

public interface ICierreService
{
    Task<CierreResumenDto> ObtenerResumenAsync(CierreFiltroDto filtro);

    Task<List<CierreDto>> ObtenerCierresAsync(CierreFiltroDto filtro);

    Task<List<CierreProveedorListadoDto>> ObtenerCierresProveedorAsync();

    Task<CierreProveedorBorradorDto> GenerarBorradorProveedorAsync(CierreFiltroDto filtro);

    Task<ResultadoOperacionDto> GuardarBorradorProveedorAsync(ConfirmarCierreProveedorDto input);

    Task<ResultadoOperacionDto> ConfirmarLiquidacionProveedorAsync(ConfirmarCierreProveedorDto input);

    Task<ResultadoOperacionDto> EliminarBorradorProveedorAsync(int cierreProveedorId);

    Task<byte[]> GenerarExcelProveedorAsync(int cierreProveedorId);
}
