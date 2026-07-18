using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRespuestasFlotaChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RespuestasFlotaChat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FlotaChatUsuarioId = table.Column<int>(type: "int", nullable: false),
                    ConductorId = table.Column<int>(type: "int", nullable: true),
                    GrupoId = table.Column<int>(type: "int", nullable: false),
                    Contenido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRecibido = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Procesado = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasFlotaChat", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespuestasFlotaChat");
        }
    }
}
