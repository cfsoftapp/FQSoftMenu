using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class RolSistemaConfiguration : IEntityTypeConfiguration<RolSistema>
{
    public void Configure(EntityTypeBuilder<RolSistema> entity)
    {
        entity.ToTable("RolesSistema");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.Codigo)
            .IsUnique();
    }
}