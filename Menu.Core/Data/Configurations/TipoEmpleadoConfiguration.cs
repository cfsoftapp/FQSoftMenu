using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class TipoEmpleadoConfiguration : IEntityTypeConfiguration<TipoEmpleado>
{
    public void Configure(EntityTypeBuilder<TipoEmpleado> entity)
    {
        entity.ToTable("TiposEmpleado");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Nombre)
            .HasMaxLength(80)
            .IsRequired();

        entity.Property(e => e.Descripcion)
            .HasMaxLength(200);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.Nombre)
            .IsUnique();
    }
}
