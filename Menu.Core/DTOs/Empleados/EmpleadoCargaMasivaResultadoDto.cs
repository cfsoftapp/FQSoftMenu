namespace Menu.DTOs.Empleados;

public class EmpleadoCargaMasivaResultadoDto
{
    public int NumeroFila { get; set; }

    public string Dni { get; set; } = string.Empty;

    public string Trabajador { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;

    public bool Importado { get; set; }
}
