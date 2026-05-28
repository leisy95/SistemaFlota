using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregaTrazabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrazabilidadFacturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AutorizacionId = table.Column<int>(type: "int", nullable: true),
                    FacturaRemision = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cliente = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Conductor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Transportadora = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Guia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Vehiculo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PesoKilos = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ValorFlete = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    AjusteRecibido = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FacturaEntregada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Novedad = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrazabilidadFacturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrazabilidadFacturas_Autorizaciones_AutorizacionId",
                        column: x => x.AutorizacionId,
                        principalTable: "Autorizaciones",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotasTrazabilidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TrazabilidadId = table.Column<int>(type: "int", nullable: false),
                    NumeroNota = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cliente = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Conductor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FacturaEntregada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Observacion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasTrazabilidad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasTrazabilidad_TrazabilidadFacturas_TrazabilidadId",
                        column: x => x.TrazabilidadId,
                        principalTable: "TrazabilidadFacturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_NotasTrazabilidad_TrazabilidadId",
                table: "NotasTrazabilidad",
                column: "TrazabilidadId");

            migrationBuilder.CreateIndex(
                name: "IX_TrazabilidadFacturas_AutorizacionId",
                table: "TrazabilidadFacturas",
                column: "AutorizacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotasTrazabilidad");

            migrationBuilder.DropTable(
                name: "TrazabilidadFacturas");
        }
    }
}
