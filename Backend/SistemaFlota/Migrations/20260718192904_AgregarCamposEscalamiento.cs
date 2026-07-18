using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposEscalamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Escalado",
                table: "Autorizaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaUltimoRecordatorio",
                table: "Autorizaciones",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntentosRecordatorio",
                table: "Autorizaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Escalado",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "FechaUltimoRecordatorio",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "IntentosRecordatorio",
                table: "Autorizaciones");
        }
    }
}
