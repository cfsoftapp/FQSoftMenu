using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionConsumosActualizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ObservacionAnulacion",
                table: "ConsumosMenu",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TipoMotivoAnulacion",
                table: "ConsumosMenu",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivoAnulacionConsumo",
                table: "ConsumosAdicionales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionAnulacion",
                table: "ConsumosAdicionales",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObservacionAnulacion",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "TipoMotivoAnulacion",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacionConsumo",
                table: "ConsumosAdicionales");

            migrationBuilder.DropColumn(
                name: "ObservacionAnulacion",
                table: "ConsumosAdicionales");
        }
    }
}
