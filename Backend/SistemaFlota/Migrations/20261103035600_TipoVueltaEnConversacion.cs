using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class TipoVueltaEnConversacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoVueltaTemp",
                table: "ConversacionesFlotaChat",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoVueltaTemp",
                table: "ConversacionesFlotaChat");
        }
    }
}
