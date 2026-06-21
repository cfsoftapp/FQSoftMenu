using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AsociarComensalesEmpresaSucursal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmpresaClienteId",
                table: "Empleados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Empleados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaClienteId",
                table: "Empleados",
                column: "EmpresaClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_SucursalId",
                table: "Empleados",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_EmpresasCliente_EmpresaClienteId",
                table: "Empleados",
                column: "EmpresaClienteId",
                principalTable: "EmpresasCliente",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Sucursales_SucursalId",
                table: "Empleados",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_EmpresasCliente_EmpresaClienteId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Sucursales_SucursalId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_EmpresaClienteId",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_SucursalId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "EmpresaClienteId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Empleados");
        }
    }
}
