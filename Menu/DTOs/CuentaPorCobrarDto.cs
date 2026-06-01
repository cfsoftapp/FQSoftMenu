using Menu.Enums;

namespace Menu.DTOs;

public class CuentaPorCobrarDto
{
    public TipoCuentaPorCobrar TipoCuenta { get; set; }

    public int ConsumoMenuId { get; set; }

    public int ConsumoAdicionalId { get; set; }

    public DateTime Fecha { get; set; }

    public int EmpleadoId { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string EmpleadoNombre { get; set; } = string.Empty;

    public string Concepto { get; set; } = string.Empty;

    public TipoServicioMenu? TipoServicio { get; set; }

    public TipoAdicional? TipoAdicional { get; set; }

    public CategoriaConsumoAdicional? Categoria { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public string UsuarioRegistroNombre { get; set; } = string.Empty;

    public bool Seleccionado { get; set; }
}