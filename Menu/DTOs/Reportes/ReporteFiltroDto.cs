namespace Menu.DTOs.Reportes
{
    public class ReporteFiltroDto
    {
        public DateTime? FechaDesde { get; set; } = DateTime.Today;
        public DateTime? FechaHasta { get; set; } = DateTime.Today;

        public string? DniTrabajador { get; set; }
        public string? NombreTrabajador { get; set; }
    }
}