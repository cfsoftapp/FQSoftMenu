using Menu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Menu.Data.Configurations;

public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> entity)
    {
        entity.ToTable("RolesPermisos");

        entity.HasKey(e => e.Id);

        entity.HasOne(e => e.RolSistema)
            .WithMany(e => e.RolPermisos)
            .HasForeignKey(e => e.RolSistemaId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.PermisoSistema)
            .WithMany(e => e.RolPermisos)
            .HasForeignKey(e => e.PermisoSistemaId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.RolSistemaId, e.PermisoSistemaId })
            .IsUnique();
    }
}