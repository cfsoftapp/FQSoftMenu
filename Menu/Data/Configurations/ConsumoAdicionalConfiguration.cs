using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations
{
    public class ConsumoAdicionalConfiguration : IEntityTypeConfiguration<ConsumoAdicional>
    {
        public void Configure(EntityTypeBuilder<ConsumoAdicional> entity)
        {
            entity.ToTable("ConsumosAdicionales");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Fecha)
                .IsRequired();

            entity.Property(e => e.TipoAdicional)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Categoria)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Descripcion)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Precio)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(e => e.FormaCobro)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.EstadoCobro)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.Observacion)
                .HasMaxLength(250);

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

            entity.Property(e => e.FechaRegistro)
                .IsRequired();

            entity.HasOne(e => e.Empleado)
                .WithMany(e => e.ConsumosAdicionales)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ConsumoMenu)
                .WithMany(e => e.ConsumosAdicionales)
                .HasForeignKey(e => e.ConsumoMenuId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}