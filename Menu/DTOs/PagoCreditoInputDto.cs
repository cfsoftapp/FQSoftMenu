using Menu.Enums;

namespace Menu.DTOs;

public class PagoCreditoInputDto
{
    public int EmpleadoId { get; set; }

    public List<int> ConsumoMenuIds { get; set; } = new();

    public List<int> ConsumoAdicionalIds { get; set; } = new();

    public FormaPagoCredito FormaPago { get; set; } = FormaPagoCredito.Efectivo;

    public DateTime FechaPago { get; set; } = DateTime.Now;

    public string? Observacion { get; set; }

    public int UsuarioRegistroId { get; set; }

    public string UsuarioRegistroNombre { get; set; } = string.Empty;
}