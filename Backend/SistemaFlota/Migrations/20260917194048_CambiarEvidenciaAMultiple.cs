using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class CambiarEvidenciaAMultiple : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenciaImagen",
                table: "SalidasNoConforme");

            migrationBuilder.RenameColumn(
                name: "EvidenciaImagenVerificacion",
                table: "SalidasNoConforme",
                newName: "EvidenciaPdfTratamiento");

            migrationBuilder.CreateTable(
                name: "SalidasNoConformeEvidencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SalidaNoConformeId = table.Column<int>(type: "int", nullable: false),
                    Paso = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreArchivo = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoArchivo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaSubida = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidasNoConformeEvidencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalidasNoConformeEvidencias_SalidasNoConforme_SalidaNoConfor~",
                        column: x => x.SalidaNoConformeId,
                        principalTable: "SalidasNoConforme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasNoConformeEvidencias_SalidaNoConformeId",
                table: "SalidasNoConformeEvidencias",
                column: "SalidaNoConformeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalidasNoConformeEvidencias");

            migrationBuilder.RenameColumn(
                name: "EvidenciaPdfTratamiento",
                table: "SalidasNoConforme",
                newName: "EvidenciaImagenVerificacion");

            migrationBuilder.AddColumn<string>(
                name: "EvidenciaImagen",
                table: "SalidasNoConforme",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
