using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class PermisoSistemaConfiguration : IEntityTypeConfiguration<PermisoSistema>
{
    public void Configure(EntityTypeBuilder<PermisoSistema> entity)
    {
        entity.ToTable("PermisosSistema");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(80);

        entity.Property(e => e.Nombre)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.Modulo)
            .IsRequired()
            .HasMaxLength(80);

        entity.HasIndex(e => e.Codigo)
            .IsUnique();
    }
}