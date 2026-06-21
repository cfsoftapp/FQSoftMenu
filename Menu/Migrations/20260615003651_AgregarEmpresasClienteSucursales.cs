using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEmpresasClienteSucursales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmpresasCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreComercial = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Ruc = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasCliente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EmpresaClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sucursales_EmpresasCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresasCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasCliente_NombreComercial",
                table: "EmpresasCliente",
                column: "NombreComercial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasCliente_Ruc",
                table: "EmpresasCliente",
                column: "Ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_EmpresaClienteId",
                table: "Sucursales",
                column: "EmpresaClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Sucursales_Nombre_EmpresaClienteId",
                table: "Sucursales",
                columns: new[] { "Nombre", "EmpresaClienteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropTable(
                name: "EmpresasCliente");
        }
    }
}
