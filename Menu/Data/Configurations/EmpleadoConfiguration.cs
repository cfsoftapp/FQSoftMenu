using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> entity)
    {
        entity.ToTable("Empleados");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Dni)
            .IsRequired()
            .HasMaxLength(15);

        entity.Property(e => e.Nombres)
            .HasMaxLength(100)
            .IsRequired();

        entity.Property(e => e.Apellidos)
            .HasMaxLength(100)
            .IsRequired();

        entity.Ignore(e => e.NombreCompleto);

        entity.Property(e => e.Estado)
            .IsRequired();

        entity.Property(e => e.Categoria)
            .HasConversion<int>()
            .IsRequired();

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.Dni)
            .IsUnique();
    }
}