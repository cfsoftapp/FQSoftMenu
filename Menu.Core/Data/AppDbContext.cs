using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empleado> Empleados => Set<Empleado>();
        public DbSet<TipoEmpleado> TiposEmpleado => Set<TipoEmpleado>();
        public DbSet<EmpresaCliente> EmpresasCliente => Set<EmpresaCliente>();
        public DbSet<Sucursal> Sucursales => Set<Sucursal>();
        public DbSet<ConfiguracionMenu> ConfiguracionesMenu => Set<ConfiguracionMenu>();
        public DbSet<ConsumoMenu> ConsumosMenu => Set<ConsumoMenu>();
        public DbSet<ConsumoAdicional> ConsumosAdicionales => Set<ConsumoAdicional>();
        public DbSet<PagoConsumoAdicional> PagosConsumosAdicionales => Set<PagoConsumoAdicional>();
        public DbSet<PagoConsumoAdicionalDetalle> PagosConsumosAdicionalesDetalle => Set<PagoConsumoAdicionalDetalle>();
        public DbSet<UsuarioSistema> UsuariosSistema => Set<UsuarioSistema>();

        public DbSet<RolSistema> RolesSistema => Set<RolSistema>();
        public DbSet<PermisoSistema> PermisosSistema => Set<PermisoSistema>();
        public DbSet<RolPermiso> RolesPermisos => Set<RolPermiso>();
        public DbSet<CierreProveedor> CierresProveedor => Set<CierreProveedor>();
        public DbSet<CierreProveedorDetalle> CierresProveedorDetalle => Set<CierreProveedorDetalle>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
