using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations
{
    public class PagoConsumoAdicionalConfiguration : IEntityTypeConfiguration<PagoConsumoAdicional>
    {
        public void Configure(EntityTypeBuilder<PagoConsumoAdicional> entity)
        {
            entity.ToTable("PagosConsumosAdicionales");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.FechaPago)
                .IsRequired();

            entity.Property(e => e.FormaPago)
                .IsRequired();

            entity.Property(e => e.MontoPagado)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.Observacion)
                .HasMaxLength(250);

            entity.Property(e => e.FechaRegistro)
                .IsRequired();

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.PagosAdicionales)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.UsuarioRegistroId)
                .IsRequired();

            entity.Property(e => e.UsuarioRegistroNombre)
                .IsRequired()
                .HasMaxLength(120);
        }
    }
}