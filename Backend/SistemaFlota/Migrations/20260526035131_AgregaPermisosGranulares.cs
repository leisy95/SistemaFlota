using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregaPermisosGranulares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PuedeCrear",
                table: "UsuarioPermisos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PuedeEditar",
                table: "UsuarioPermisos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PuedeEliminar",
                table: "UsuarioPermisos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PuedeVer",
                table: "UsuarioPermisos",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuedeCrear",
                table: "UsuarioPermisos");

            migrationBuilder.DropColumn(
                name: "PuedeEditar",
                table: "UsuarioPermisos");

            migrationBuilder.DropColumn(
                name: "PuedeEliminar",
                table: "UsuarioPermisos");

            migrationBuilder.DropColumn(
                name: "PuedeVer",
                table: "UsuarioPermisos");
        }
    }
}
