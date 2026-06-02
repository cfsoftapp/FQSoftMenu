using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAdicionalEmpresaCierreProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Concepto",
                table: "CierresProveedorDetalle",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ConsumoAdicionalId",
                table: "CierresProveedorDetalle",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoAdicional",
                table: "CierresProveedorDetalle",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CierresProveedorDetalle_ConsumoAdicionalId",
                table: "CierresProveedorDetalle",
                column: "ConsumoAdicionalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CierresProveedorDetalle_ConsumoAdicionalId",
                table: "CierresProveedorDetalle");

            migrationBuilder.DropColumn(
                name: "Concepto",
                table: "CierresProveedorDetalle");

            migrationBuilder.DropColumn(
                name: "ConsumoAdicionalId",
                table: "CierresProveedorDetalle");

            migrationBuilder.DropColumn(
                name: "TipoAdicional",
                table: "CierresProveedorDetalle");
        }
    }
}
