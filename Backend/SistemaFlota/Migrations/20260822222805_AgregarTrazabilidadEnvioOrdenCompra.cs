using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTrazabilidadEnvioOrdenCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CorreoEnviado",
                table: "OrdenesCompra",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEnvioCorreo",
                table: "OrdenesCompra",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioEnvioCorreoId",
                table: "OrdenesCompra",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompra_UsuarioEnvioCorreoId",
                table: "OrdenesCompra",
                column: "UsuarioEnvioCorreoId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesCompra_Usuarios_UsuarioEnvioCorreoId",
                table: "OrdenesCompra",
                column: "UsuarioEnvioCorreoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesCompra_Usuarios_UsuarioEnvioCorreoId",
                table: "OrdenesCompra");

            migrationBuilder.DropIndex(
                name: "IX_OrdenesCompra_UsuarioEnvioCorreoId",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "CorreoEnviado",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "FechaEnvioCorreo",
                table: "OrdenesCompra");

            migrationBuilder.DropColumn(
                name: "UsuarioEnvioCorreoId",
                table: "OrdenesCompra");
        }
    }
}
