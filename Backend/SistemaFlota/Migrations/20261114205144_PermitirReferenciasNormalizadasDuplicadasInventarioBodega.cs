using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class PermitirReferenciasNormalizadasDuplicadasInventarioBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventariosBodega_ReferenciaNormalizada",
                table: "InventariosBodega");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosBodega_ReferenciaNormalizada",
                table: "InventariosBodega",
                column: "ReferenciaNormalizada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InventariosBodega_ReferenciaNormalizada",
                table: "InventariosBodega");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosBodega_ReferenciaNormalizada",
                table: "InventariosBodega",
                column: "ReferenciaNormalizada",
                unique: true);
        }
    }
}
