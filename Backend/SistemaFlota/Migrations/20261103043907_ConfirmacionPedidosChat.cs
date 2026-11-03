using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmacionPedidosChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvisoMensajeId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConfirmadoPorFlotaChatUsuarioId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreadoPorUsuarioId",
                table: "Pedidos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacion",
                table: "Pedidos",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvisoMensajeId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "ConfirmadoPorFlotaChatUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "CreadoPorUsuarioId",
                table: "Pedidos");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacion",
                table: "Pedidos");
        }
    }
}
