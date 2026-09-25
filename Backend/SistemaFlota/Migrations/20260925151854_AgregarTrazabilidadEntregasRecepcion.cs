using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTrazabilidadEntregasRecepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumeroUltimaEntrega",
                table: "RecepcionesMercancias",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "RecepcionesMercanciaDetalle",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "NumeroEntrega",
                table: "RecepcionesMercanciaDetalle",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ProcesadoInventario",
                table: "RecepcionesMercanciaDetalle",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            // Los detalles que ya existen corresponden a mercancía
            // procesada por el sistema anterior.
            migrationBuilder.Sql(@"
                UPDATE RecepcionesMercanciaDetalle d
                INNER JOIN RecepcionesMercancias r
                    ON r.Id = d.RecepcionMercanciaId
                SET
                    d.ProcesadoInventario = 1,
                    d.NumeroEntrega = 1,
                    d.FechaEntrega = r.FechaRecepcion;
            ");

            // Las recepciones existentes se consideran con una primera entrega.
            migrationBuilder.Sql(@"
                UPDATE RecepcionesMercancias
                SET NumeroUltimaEntrega = 1
                WHERE NumeroUltimaEntrega = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumeroUltimaEntrega",
                table: "RecepcionesMercancias");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "RecepcionesMercanciaDetalle");

            migrationBuilder.DropColumn(
                name: "NumeroEntrega",
                table: "RecepcionesMercanciaDetalle");

            migrationBuilder.DropColumn(
                name: "ProcesadoInventario",
                table: "RecepcionesMercanciaDetalle");
        }
    }
}