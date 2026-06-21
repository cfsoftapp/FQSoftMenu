using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> entity)
    {
        entity.ToTable("Sucursales");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Nombre)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(e => e.Direccion)
            .HasMaxLength(200);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasOne(e => e.EmpresaCliente)
            .WithMany(e => e.Sucursales)
            .HasForeignKey(e => e.EmpresaClienteId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(e => new { e.Nombre, e.EmpresaClienteId })
            .IsUnique();
    }
}
