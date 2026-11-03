using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class EventosFlotaChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdEvento",
                table: "RespuestasFlotaChat",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MensajeId",
                table: "RespuestasFlotaChat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoMensaje",
                table: "RespuestasFlotaChat",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasFlotaChat_IdEvento",
                table: "RespuestasFlotaChat",
                column: "IdEvento",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RespuestasFlotaChat_IdEvento",
                table: "RespuestasFlotaChat");

            migrationBuilder.DropColumn(
                name: "IdEvento",
                table: "RespuestasFlotaChat");

            migrationBuilder.DropColumn(
                name: "MensajeId",
                table: "RespuestasFlotaChat");

            migrationBuilder.DropColumn(
                name: "TipoMensaje",
                table: "RespuestasFlotaChat");
        }
    }
}
