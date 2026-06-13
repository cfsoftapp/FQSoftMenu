namespace Menu.Models
{
    public class PagoConsumoAdicionalDetalle
    {
        public int Id { get; set; }

        public int PagoConsumoAdicionalId { get; set; }

        public PagoConsumoAdicional PagoConsumoAdicional { get; set; } = null!;

        public int ConsumoAdicionalId { get; set; }

        public ConsumoAdicional ConsumoAdicional { get; set; } = null!;

        public decimal MontoAplicado { get; set; }
    }
}
