namespace Menu.DTOs.RegistroDiario;

public class ConsumoDiaDto
{
    public int Id { get; set; }

    public string Origen { get; set; } = string.Empty;
    // MenuPrincipal / Adicional

    public string Tipo { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string FormaPago { get; set; } = string.Empty;

    public string EstadoCobro { get; set; } = string.Empty;

    public decimal Importe { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Anulado { get; set; }

    public string? MotivoAnulacion { get; set; }

    public bool PuedeEditar { get; set; }

    public bool PuedeAnular { get; set; }
}