using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class ConfiguracionMenuConfiguration : IEntityTypeConfiguration<ConfiguracionMenu>
{
    public void Configure(EntityTypeBuilder<ConfiguracionMenu> entity)
    {
        entity.ToTable("ConfiguracionMenu");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.PrecioMenu)
            .HasPrecision(18, 2)
            .IsRequired();

        entity.Property(e => e.Moneda)
            .IsRequired()
            .HasMaxLength(3);

        entity.Property(e => e.FechaActualizacion)
            .IsRequired();
    }
}