namespace Menu.DTOs.RegistroDiario;

public class RegistroComensalRecienteDto
{
    public int ConsumoMenuId { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string EmpresaCliente { get; set; } = string.Empty;

    public string Sucursal { get; set; } = string.Empty;

    public string TipoServicio { get; set; } = string.Empty;

    public string EstadoComensal { get; set; } = string.Empty;

    public string FormaCobro { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public bool Anulado { get; set; }
}
