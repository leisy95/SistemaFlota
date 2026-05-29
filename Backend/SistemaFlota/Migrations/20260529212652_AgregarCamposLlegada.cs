using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposLlegada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EstadoVehiculoLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacionLlegada",
                table: "Autorizaciones",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaReporteLlegada",
                table: "Autorizaciones",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaPorteriaLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FotoOdometroLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "KilometrajeFinal",
                table: "Autorizaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NovedadesViaje",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ObservacionPorteriaLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioPorteriaLlegada",
                table: "Autorizaciones",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "EstadoVehiculoLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacionLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "FechaReporteLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "FirmaPorteriaLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "FotoOdometroLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "KilometrajeFinal",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "NovedadesViaje",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "ObservacionPorteriaLlegada",
                table: "Autorizaciones");

            migrationBuilder.DropColumn(
                name: "UsuarioPorteriaLlegada",
                table: "Autorizaciones");
        }
    }
}
