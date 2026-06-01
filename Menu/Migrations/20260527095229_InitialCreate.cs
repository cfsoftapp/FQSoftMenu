using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrecioMenu = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionMenu", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Dni = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermisosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumosMenu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoServicio = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpleadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioMenu = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TipoPagoMenu = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    UsuarioRegistroId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioRegistroNombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosMenu_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosConsumosAdicionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpleadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FormaPago = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    UsuarioRegistroId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioRegistroNombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosConsumosAdicionales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosConsumosAdicionales_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RolSistemaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermisoSistemaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_PermisosSistema_PermisoSistemaId",
                        column: x => x.PermisoSistemaId,
                        principalTable: "PermisosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermisos_RolesSistema_RolSistemaId",
                        column: x => x.RolSistemaId,
                        principalTable: "RolesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ClaveHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RolSistemaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSistema_RolesSistema_RolSistemaId",
                        column: x => x.RolSistemaId,
                        principalTable: "RolesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsumosAdicionales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmpleadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoMenuId = table.Column<int>(type: "INTEGER", nullable: true),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    FormaCobro = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoCobro = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    UsuarioRegistroId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioRegistroNombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosAdicionales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosAdicionales_ConsumosMenu_ConsumoMenuId",
                        column: x => x.ConsumoMenuId,
                        principalTable: "ConsumosMenu",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ConsumosAdicionales_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosConsumosAdicionalesDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PagoConsumoAdicionalId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoAdicionalId = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoAplicado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosConsumosAdicionalesDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosConsumosAdicionalesDetalle_ConsumosAdicionales_ConsumoAdicionalId",
                        column: x => x.ConsumoAdicionalId,
                        principalTable: "ConsumosAdicionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosConsumosAdicionalesDetalle_PagosConsumosAdicionales_PagoConsumoAdicionalId",
                        column: x => x.PagoConsumoAdicionalId,
                        principalTable: "PagosConsumosAdicionales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosAdicionales_ConsumoMenuId",
                table: "ConsumosAdicionales",
                column: "ConsumoMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosAdicionales_EmpleadoId",
                table: "ConsumosAdicionales",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosMenu_EmpleadoId",
                table: "ConsumosMenu",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosMenu_Fecha_EmpleadoId_TipoServicio",
                table: "ConsumosMenu",
                columns: new[] { "Fecha", "EmpleadoId", "TipoServicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Dni",
                table: "Empleados",
                column: "Dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosConsumosAdicionales_EmpleadoId",
                table: "PagosConsumosAdicionales",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosConsumosAdicionalesDetalle_ConsumoAdicionalId",
                table: "PagosConsumosAdicionalesDetalle",
                column: "ConsumoAdicionalId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosConsumosAdicionalesDetalle_PagoConsumoAdicionalId",
                table: "PagosConsumosAdicionalesDetalle",
                column: "PagoConsumoAdicionalId");

            migrationBuilder.CreateIndex(
                name: "IX_PermisosSistema_Codigo",
                table: "PermisosSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_PermisoSistemaId",
                table: "RolesPermisos",
                column: "PermisoSistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermisos_RolSistemaId_PermisoSistemaId",
                table: "RolesPermisos",
                columns: new[] { "RolSistemaId", "PermisoSistemaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesSistema_Codigo",
                table: "RolesSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_NombreUsuario",
                table: "UsuariosSistema",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_RolSistemaId",
                table: "UsuariosSistema",
                column: "RolSistemaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionMenu");

            migrationBuilder.DropTable(
                name: "PagosConsumosAdicionalesDetalle");

            migrationBuilder.DropTable(
                name: "RolesPermisos");

            migrationBuilder.DropTable(
                name: "UsuariosSistema");

            migrationBuilder.DropTable(
                name: "ConsumosAdicionales");

            migrationBuilder.DropTable(
                name: "PagosConsumosAdicionales");

            migrationBuilder.DropTable(
                name: "PermisosSistema");

            migrationBuilder.DropTable(
                name: "RolesSistema");

            migrationBuilder.DropTable(
                name: "ConsumosMenu");

            migrationBuilder.DropTable(
                name: "Empleados");
        }
    }
}
