using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposTemporalesConversacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinoTemp",
                table: "ConversacionesFlotaChat",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VehiculoIdTemp",
                table: "ConversacionesFlotaChat",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinoTemp",
                table: "ConversacionesFlotaChat");

            migrationBuilder.DropColumn(
                name: "VehiculoIdTemp",
                table: "ConversacionesFlotaChat");
        }
    }
}
