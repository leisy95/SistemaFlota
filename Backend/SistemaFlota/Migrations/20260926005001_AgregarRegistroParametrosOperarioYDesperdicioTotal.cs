using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRegistroParametrosOperarioYDesperdicioTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DesperdicioTotalKg",
                table: "RegistrosFormatoCalidad",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RegistrosParametrosOperario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RegistroFormatoCalidadId = table.Column<int>(type: "int", nullable: false),
                    OperarioNombre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VariablesCriticasJson = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaGuardado = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MotivoCambio = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosParametrosOperario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosParametrosOperario_RegistrosFormatoCalidad_Registro~",
                        column: x => x.RegistroFormatoCalidadId,
                        principalTable: "RegistrosFormatoCalidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosParametrosOperario_RegistroFormatoCalidadId",
                table: "RegistrosParametrosOperario",
                column: "RegistroFormatoCalidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosParametrosOperario");

            migrationBuilder.DropColumn(
                name: "DesperdicioTotalKg",
                table: "RegistrosFormatoCalidad");
        }
    }
}
