using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class ChecklistItemIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspeccionDetalles_ChecklistItems_ChecklistItemId",
                table: "InspeccionDetalles");

            migrationBuilder.AlterColumn<int>(
                name: "ChecklistItemId",
                table: "InspeccionDetalles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DescripcionItem",
                table: "InspeccionDetalles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_InspeccionDetalles_ChecklistItems_ChecklistItemId",
                table: "InspeccionDetalles",
                column: "ChecklistItemId",
                principalTable: "ChecklistItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InspeccionDetalles_ChecklistItems_ChecklistItemId",
                table: "InspeccionDetalles");

            migrationBuilder.DropColumn(
                name: "DescripcionItem",
                table: "InspeccionDetalles");

            migrationBuilder.AlterColumn<int>(
                name: "ChecklistItemId",
                table: "InspeccionDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InspeccionDetalles_ChecklistItems_ChecklistItemId",
                table: "InspeccionDetalles",
                column: "ChecklistItemId",
                principalTable: "ChecklistItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
