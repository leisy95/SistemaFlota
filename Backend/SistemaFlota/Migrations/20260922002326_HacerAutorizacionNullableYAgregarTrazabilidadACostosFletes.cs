using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class HacerAutorizacionNullableYAgregarTrazabilidadACostosFletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
               name: "costosfletes_ibfk_1",
               table: "CostosFletes");

            migrationBuilder.AlterColumn<int>(
                name: "AutorizacionId",
                table: "CostosFletes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "TrazabilidadId",
                table: "CostosFletes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CostosFletes_TrazabilidadId",
                table: "CostosFletes",
                column: "TrazabilidadId");

            migrationBuilder.AddForeignKey(
                name: "FK_CostosFletes_Autorizaciones_AutorizacionId",
                table: "CostosFletes",
                column: "AutorizacionId",
                principalTable: "Autorizaciones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CostosFletes_TrazabilidadFacturas_TrazabilidadId",
                table: "CostosFletes",
                column: "TrazabilidadId",
                principalTable: "TrazabilidadFacturas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
               name: "costosfletes_ibfk_1",
               table: "CostosFletes");

            migrationBuilder.DropForeignKey(
                name: "FK_CostosFletes_TrazabilidadFacturas_TrazabilidadId",
                table: "CostosFletes");

            migrationBuilder.DropIndex(
                name: "IX_CostosFletes_TrazabilidadId",
                table: "CostosFletes");

            migrationBuilder.DropColumn(
                name: "TrazabilidadId",
                table: "CostosFletes");

            migrationBuilder.AlterColumn<int>(
                name: "AutorizacionId",
                table: "CostosFletes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CostosFletes_Autorizaciones_AutorizacionId",
                table: "CostosFletes",
                column: "AutorizacionId",
                principalTable: "Autorizaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
