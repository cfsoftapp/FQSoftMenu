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

        entity.Ignore(e => e.TipoEmpleadoNombre);

        entity.Ignore(e => e.EmpresaClienteNombre);

        entity.Ignore(e => e.SucursalNombre);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.Dni)
            .IsUnique();

        entity.HasOne(e => e.TipoEmpleado)
            .WithMany(e => e.Empleados)
            .HasForeignKey(e => e.TipoEmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.EmpresaCliente)
            .WithMany(e => e.Empleados)
            .HasForeignKey(e => e.EmpresaClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(e => e.Sucursal)
            .WithMany(e => e.Empleados)
            .HasForeignKey(e => e.SucursalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
