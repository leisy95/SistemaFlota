using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarClienteIdMovimientoInventarioBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "MovimientosInventarioBodega",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventarioBodega_ClienteId",
                table: "MovimientosInventarioBodega",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventarioBodega_Clientes_ClienteId",
                table: "MovimientosInventarioBodega",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventarioBodega_Clientes_ClienteId",
                table: "MovimientosInventarioBodega");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventarioBodega_ClienteId",
                table: "MovimientosInventarioBodega");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "MovimientosInventarioBodega");
        }
    }
}
