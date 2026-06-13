using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class CierreProveedorConfiguration : IEntityTypeConfiguration<CierreProveedor>
{
    public void Configure(EntityTypeBuilder<CierreProveedor> entity)
    {
        entity.ToTable("CierresProveedor");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.FechaDesde)
            .IsRequired();

        entity.Property(e => e.FechaHasta)
            .IsRequired();

        entity.Property(e => e.Estado)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.TotalPersonalActivo)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.TotalPlanilla)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.TotalExcluidoRevision)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.TotalLiquidarProveedor)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.Observacion)
            .HasMaxLength(300);

        entity.Property(e => e.UsuarioConfirmacionNombre)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.FechaConfirmacion)
            .IsRequired();

        entity.HasIndex(e => new { e.FechaDesde, e.FechaHasta })
            .IsUnique();
    }
}
