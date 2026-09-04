using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdempotencyLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation(
                            "MySql:ValueGenerationStrategy",
                            MySqlValueGenerationStrategy.IdentityColumn),

                    Key = table.Column<string>(
                        type: "varchar(255)",
                        maxLength: 255,
                        nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Method = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    Path = table.Column<string>(
                        type: "longtext",
                        nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    UsuarioId = table.Column<string>(
                        type: "longtext",
                        nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    StatusCode = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ContentType = table.Column<string>(
                        type: "longtext",
                        nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    ResponseBody = table.Column<string>(
                        type: "longtext",
                        nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime(6)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdempotencyLogs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyLogs_Key",
                table: "IdempotencyLogs",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdempotencyLogs");
        }
    }
}