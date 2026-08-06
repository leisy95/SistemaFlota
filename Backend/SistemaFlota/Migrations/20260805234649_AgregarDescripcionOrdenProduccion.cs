using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDescripcionOrdenProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "OrdenesProduccionExternas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "OrdenesProduccionExternas");
        }
    }
}
