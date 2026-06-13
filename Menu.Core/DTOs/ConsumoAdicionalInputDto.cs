using Menu.Enums;

namespace Menu.DTOs;

public class ConsumoAdicionalInputDto
{
    public TipoAdicional TipoAdicional { get; set; } = TipoAdicional.Producto;

    public CategoriaConsumoAdicional Categoria { get; set; } = CategoriaConsumoAdicional.Bebida;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Precio { get; set; }

    public FormaCobroAdicional FormaCobro { get; set; } = FormaCobroAdicional.Efectivo;
}