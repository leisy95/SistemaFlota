using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class CrearModuloInventarioBodegaCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventariosBodega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Referencia = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipodeBolsa = table.Column<string>(name: "Tipo de Bolsa", type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoMaterial = table.Column<string>(name: "Tipo Material", type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Densidad = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Segundocolor = table.Column<string>(name: "Segundo color", type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImpresoNo = table.Column<string>(name: "Impreso/No", type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Troquel = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenciaImpresa = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ancho = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Alto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Calibre = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    FuelleLateral = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelleFondo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FuelleSuperior = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaqueteNumero = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaqueteCantidad = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ReferenciaNormalizada = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosBodega", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalidasBodega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Usuario = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FechaCancelacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClienteId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidasBodega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalidasBodega_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventariosBodegaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdInventarioBodega = table.Column<int>(type: "int", nullable: false),
                    ReferenciaPeso = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PesoActual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaRegistroItem = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosBodegaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosBodegaItems_InventariosBodega_IdInventarioBodega",
                        column: x => x.IdInventarioBodega,
                        principalTable: "InventariosBodega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MovimientosInventarioBodega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Referencia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReferenciaPeso = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Peso = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Usuario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdInventarioBodega = table.Column<int>(type: "int", nullable: false),
                    IdInventarioBodegaItem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosInventarioBodega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosInventarioBodega_InventariosBodegaItems_IdInventa~",
                        column: x => x.IdInventarioBodegaItem,
                        principalTable: "InventariosBodegaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosInventarioBodega_InventariosBodega_IdInventarioBo~",
                        column: x => x.IdInventarioBodega,
                        principalTable: "InventariosBodega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SalidasBodegaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SalidaBodegaId = table.Column<int>(type: "int", nullable: false),
                    InventarioBodegaItemId = table.Column<int>(type: "int", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalidasBodegaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalidasBodegaDetalles_InventariosBodegaItems_InventarioBodeg~",
                        column: x => x.InventarioBodegaItemId,
                        principalTable: "InventariosBodegaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalidasBodegaDetalles_SalidasBodega_SalidaBodegaId",
                        column: x => x.SalidaBodegaId,
                        principalTable: "SalidasBodega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosBodega_ReferenciaNormalizada",
                table: "InventariosBodega",
                column: "ReferenciaNormalizada",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventariosBodegaItems_IdInventarioBodega",
                table: "InventariosBodegaItems",
                column: "IdInventarioBodega");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventarioBodega_Fecha",
                table: "MovimientosInventarioBodega",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventarioBodega_IdInventarioBodega",
                table: "MovimientosInventarioBodega",
                column: "IdInventarioBodega");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventarioBodega_IdInventarioBodegaItem",
                table: "MovimientosInventarioBodega",
                column: "IdInventarioBodegaItem");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasBodega_ClienteId",
                table: "SalidasBodega",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasBodega_Estado",
                table: "SalidasBodega",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasBodega_FechaCreacion",
                table: "SalidasBodega",
                column: "FechaCreacion");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasBodegaDetalles_InventarioBodegaItemId",
                table: "SalidasBodegaDetalles",
                column: "InventarioBodegaItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalidasBodegaDetalles_SalidaBodegaId",
                table: "SalidasBodegaDetalles",
                column: "SalidaBodegaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MovimientosInventarioBodega");

            migrationBuilder.DropTable(
                name: "SalidasBodegaDetalles");

            migrationBuilder.DropTable(
                name: "InventariosBodegaItems");

            migrationBuilder.DropTable(
                name: "SalidasBodega");

            migrationBuilder.DropTable(
                name: "InventariosBodega");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
