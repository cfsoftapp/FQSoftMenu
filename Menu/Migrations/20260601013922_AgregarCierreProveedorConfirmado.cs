using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCierreProveedorConfirmado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CierresProveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FechaDesde = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaHasta = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMenusActivos = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMenusPlanilla = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalMenusPlanillaExcluidos = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalPersonalActivo = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalPlanilla = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalExcluidoRevision = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TotalLiquidarProveedor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    UsuarioConfirmacionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioConfirmacionNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresProveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CierresProveedorDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CierreProveedorId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConsumoMenuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EmpleadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dni = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    EmpleadoNombre = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TipoServicio = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoPagoMenu = table.Column<int>(type: "INTEGER", nullable: false),
                    Importe = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    IncluidoProveedor = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExcluidoPorPagoDirecto = table.Column<bool>(type: "INTEGER", nullable: false),
                    MotivoExclusion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CierresProveedorDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CierresProveedorDetalle_CierresProveedor_CierreProveedorId",
                        column: x => x.CierreProveedorId,
                        principalTable: "CierresProveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CierresProveedor_FechaDesde_FechaHasta",
                table: "CierresProveedor",
                columns: new[] { "FechaDesde", "FechaHasta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CierresProveedorDetalle_CierreProveedorId",
                table: "CierresProveedorDetalle",
                column: "CierreProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_CierresProveedorDetalle_ConsumoMenuId",
                table: "CierresProveedorDetalle",
                column: "ConsumoMenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CierresProveedorDetalle");

            migrationBuilder.DropTable(
                name: "CierresProveedor");
        }
    }
}
