using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> entity)
    {
        entity.ToTable("UsuariosSistema");

        entity.HasKey(e => e.Id);

        entity.Property(e => e.NombreUsuario)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(e => e.NombreCompleto)
            .IsRequired()
            .HasMaxLength(150);

        entity.Property(e => e.ClaveHash)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(e => e.Activo)
            .IsRequired();

        entity.Property(e => e.FechaCreacion)
            .IsRequired();

        entity.HasIndex(e => e.NombreUsuario)
            .IsUnique();

        entity.HasOne(e => e.RolSistema)
            .WithMany(e => e.Usuarios)
            .HasForeignKey(e => e.RolSistemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}