using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAnulacionConsumos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsumosMenu_Fecha_EmpleadoId_TipoServicio",
                table: "ConsumosMenu");

            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "ConsumosMenu",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "ConsumosMenu",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "ConsumosMenu",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioAnulacionId",
                table: "ConsumosMenu",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAnulacionNombre",
                table: "ConsumosMenu",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "ConsumosAdicionales",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacion",
                table: "ConsumosAdicionales",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "ConsumosAdicionales",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioAnulacionId",
                table: "ConsumosAdicionales",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAnulacionNombre",
                table: "ConsumosAdicionales",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosMenu_Fecha_EmpleadoId_TipoServicio",
                table: "ConsumosMenu",
                columns: new[] { "Fecha", "EmpleadoId", "TipoServicio" },
                unique: true,
                filter: "\"Anulado\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConsumosMenu_Fecha_EmpleadoId_TipoServicio",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionId",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionNombre",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "ConsumosAdicionales");

            migrationBuilder.DropColumn(
                name: "FechaAnulacion",
                table: "ConsumosAdicionales");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "ConsumosAdicionales");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionId",
                table: "ConsumosAdicionales");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulacionNombre",
                table: "ConsumosAdicionales");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosMenu_Fecha_EmpleadoId_TipoServicio",
                table: "ConsumosMenu",
                columns: new[] { "Fecha", "EmpleadoId", "TipoServicio" },
                unique: true);
        }
    }
}
