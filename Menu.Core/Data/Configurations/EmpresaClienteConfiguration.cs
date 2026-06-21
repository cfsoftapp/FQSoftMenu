using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class EmpresaClienteConfiguration : IEntityTypeConfiguration<EmpresaCliente>
{
    public void Configure(EntityTypeBuilder<EmpresaCliente> entity)
    {
        entity.ToTable("EmpresasCliente");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.NombreComercial)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(e => e.RazonSocial)
            .HasMaxLength(160);

        entity.Property(e => e.Ruc)
            .HasMaxLength(20);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.NombreComercial)
            .IsUnique();

        entity.HasIndex(e => e.Ruc)
            .IsUnique();
    }
}
