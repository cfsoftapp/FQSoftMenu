namespace Menu.DTOs.Empleados;

public class EmpleadoCargaMasivaFilaDto
{
    public int NumeroFila { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string TipoPersonalTexto { get; set; } = string.Empty;

    public string EstadoTexto { get; set; } = string.Empty;

    public string ActivoTexto { get; set; } = string.Empty;
}
