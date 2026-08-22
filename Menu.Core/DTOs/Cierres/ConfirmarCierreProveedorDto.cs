namespace Menu.DTOs.Cierres;

public class ConfirmarCierreProveedorDto
{
    public int? CierreProveedorId { get; set; }

    public DateTime FechaDesde { get; set; }

    public DateTime FechaHasta { get; set; }

    public List<CierreProveedorItemDto> Items { get; set; } = new();

    public string? Observacion { get; set; }

    public int UsuarioConfirmacionId { get; set; }

    public string UsuarioConfirmacionNombre { get; set; } = string.Empty;
}
