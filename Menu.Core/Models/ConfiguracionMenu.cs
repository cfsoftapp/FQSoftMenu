namespace Menu.Models
{
    public class ConfiguracionMenu
    {
        public int Id { get; set; }

        public decimal PrecioMenu { get; set; }

        public string Moneda { get; set; } = "PEN";

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}
