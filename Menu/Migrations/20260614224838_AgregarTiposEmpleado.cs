using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTiposEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoEmpleadoId",
                table: "Empleados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TiposEmpleado",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEmpleado", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_TipoEmpleadoId",
                table: "Empleados",
                column: "TipoEmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposEmpleado_Nombre",
                table: "TiposEmpleado",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_TiposEmpleado_TipoEmpleadoId",
                table: "Empleados",
                column: "TipoEmpleadoId",
                principalTable: "TiposEmpleado",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_TiposEmpleado_TipoEmpleadoId",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "TiposEmpleado");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_TipoEmpleadoId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "TipoEmpleadoId",
                table: "Empleados");
        }
    }
}
