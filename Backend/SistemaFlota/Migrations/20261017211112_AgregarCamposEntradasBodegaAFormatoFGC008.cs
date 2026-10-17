using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposEntradasBodegaAFormatoFGC008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bultos",
                table: "FormatoFGC008",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CambioDespuesDeRevisar",
                table: "FormatoFGC008",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiaEntrada",
                table: "FormatoFGC008",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnProceso",
                table: "FormatoFGC008",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "KilosEntrada",
                table: "FormatoFGC008",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OperariosBodega",
                table: "FormatoFGC008",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
             name: "Origen",
             table: "FormatoFGC008",
             type: "varchar(20)",
             maxLength: 20,
             nullable: false,
             defaultValue: "Anterior")
              .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Seccion",
                table: "FormatoFGC008",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Troquel",
                table: "FormatoFGC008",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaSincronizacion",
                table: "FormatoFGC008",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UltimoNumEntrada",
                table: "FormatoFGC008",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnidadOP",
                table: "FormatoFGC008",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UnidadReal",
                table: "FormatoFGC008",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "UnidadesEntrada",
                table: "FormatoFGC008",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bultos",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "CambioDespuesDeRevisar",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "DiaEntrada",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "EnProceso",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "KilosEntrada",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "OperariosBodega",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "Origen",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "Seccion",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "Troquel",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "UltimaSincronizacion",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "UltimoNumEntrada",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "UnidadOP",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "UnidadReal",
                table: "FormatoFGC008");

            migrationBuilder.DropColumn(
                name: "UnidadesEntrada",
                table: "FormatoFGC008");
        }
    }
}
