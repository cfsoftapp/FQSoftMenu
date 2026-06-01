using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations
{
    public class PagoConsumoAdicionalDetalleConfiguration : IEntityTypeConfiguration<PagoConsumoAdicionalDetalle>
    {
        public void Configure(EntityTypeBuilder<PagoConsumoAdicionalDetalle> entity)
        {
            entity.ToTable("PagosConsumosAdicionalesDetalle");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.MontoAplicado)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.HasOne(e => e.PagoConsumoAdicional)
                .WithMany(e => e.Detalles)
                .HasForeignKey(e => e.PagoConsumoAdicionalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ConsumoAdicional)
                .WithMany(e => e.PagoDetalles)
                .HasForeignKey(e => e.ConsumoAdicionalId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
