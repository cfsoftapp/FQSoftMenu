using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Menu.Migrations
{
    /// <inheritdoc />
    public partial class AjustePagoMenuSuspendido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoCobroMenu",
                table: "ConsumosMenu",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaPagoMenu",
                table: "ConsumosMenu",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagoDirecto",
                table: "ConsumosMenu",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoCobroMenu",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "FechaPagoMenu",
                table: "ConsumosMenu");

            migrationBuilder.DropColumn(
                name: "FormaPagoDirecto",
                table: "ConsumosMenu");
        }
    }
}
