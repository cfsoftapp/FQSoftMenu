namespace Menu.DTOs.Empleados;

public class EmpleadoCargaMasivaResumenDto
{
    public int TotalFilas { get; set; }

    public int Importados { get; set; }

    public int Pendientes => Resultados.Count(x => x.Estado == "Pendiente");

    public int Observados => TotalFilas - Importados - Pendientes;

    public List<EmpleadoCargaMasivaResultadoDto> Resultados { get; set; } = new();
}
