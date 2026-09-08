using System;
using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCodigoOperario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "OpcionesFormulario",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "OpcionesFormulario");
        }
    }
}