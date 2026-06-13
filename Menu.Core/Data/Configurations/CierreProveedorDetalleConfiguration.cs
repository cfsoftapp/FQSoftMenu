using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class CierreProveedorDetalleConfiguration : IEntityTypeConfiguration<CierreProveedorDetalle>
{
    public void Configure(EntityTypeBuilder<CierreProveedorDetalle> entity)
    {
        entity.ToTable("CierresProveedorDetalle");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Dni)
            .IsRequired()
            .HasMaxLength(15);

        entity.Property(e => e.EmpleadoNombre)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.TipoServicio)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.TipoPagoMenu)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.TipoAdicional)
            .HasConversion<int>();

        entity.Property(e => e.Concepto)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(e => e.Importe)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.MotivoExclusion)
            .HasMaxLength(300);

        entity.HasOne(e => e.CierreProveedor)
            .WithMany(e => e.Detalles)
            .HasForeignKey(e => e.CierreProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.ConsumoMenuId);

        entity.HasIndex(e => e.ConsumoAdicionalId);
    }
}
