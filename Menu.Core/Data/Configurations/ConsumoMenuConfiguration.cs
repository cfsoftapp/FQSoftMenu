using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations
{
    public class ConsumoMenuConfiguration : IEntityTypeConfiguration<ConsumoMenu>
    {
        public void Configure(EntityTypeBuilder<ConsumoMenu> entity)
        {
            entity.ToTable("ConsumosMenu");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Fecha)
                .IsRequired();

            entity.Property(e => e.PrecioMenu)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.TipoServicio)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.TipoPagoMenu)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.FormaPagoDirecto)
                .HasConversion<int>();

            entity.Property(e => e.EstadoCobroMenu)
                .HasConversion<int>();

            entity.Property(e => e.FechaPagoMenu);

            entity.Property(e => e.Observacion)
                .HasMaxLength(250);

            entity.Property(e => e.FechaRegistro)
                .IsRequired();

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.ConsumosMenu)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.UsuarioRegistroId)
                .IsRequired();

            entity.Property(e => e.UsuarioRegistroNombre)
                .IsRequired()
                .HasMaxLength(120);

            entity.Property(x => x.Anulado)
                .HasDefaultValue(false);

            entity.Property(x => x.UsuarioAnulacionNombre)
                .HasMaxLength(150);

            entity.Property(x => x.MotivoAnulacion)
                .HasMaxLength(250);

            entity.HasIndex(e => new { e.Fecha, e.EmpleadoId, e.TipoServicio })
                .IsUnique()
                .HasFilter("\"Anulado\" = 0");
        }
    }
}