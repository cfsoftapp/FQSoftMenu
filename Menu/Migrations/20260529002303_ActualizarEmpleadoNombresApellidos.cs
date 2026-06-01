using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarEmpleadoNombresApellidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "Empleados");

            migrationBuilder.AddColumn<string>(
                name: "Apellidos",
                table: "Empleados",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nombres",
                table: "Empleados",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellidos",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "Nombres",
                table: "Empleados");

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "Empleados",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
