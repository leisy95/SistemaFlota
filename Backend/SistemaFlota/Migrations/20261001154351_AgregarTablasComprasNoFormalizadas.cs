using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFlota.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablasComprasNoFormalizadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CortesInventarioNoFormalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CortesInventarioNoFormalizados", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenesTrasladoNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroOrden = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Destino = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    TotalKg = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalBultos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaVerificacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioVerificacionId = table.Column<int>(type: "int", nullable: true),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioConfirmacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesTrasladoNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesTrasladoNoFormalizadas_Usuarios_UsuarioConfirmacionId",
                        column: x => x.UsuarioConfirmacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesTrasladoNoFormalizadas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesTrasladoNoFormalizadas_Usuarios_UsuarioVerificacionId",
                        column: x => x.UsuarioVerificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProveedoresNoFormalizados",
                columns: table => new
                {
                    IdProveedorNoFormalizado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Documento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Contacto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CorreoElectronico = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ciudad = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Departamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProveedoresNoFormalizados", x => x.IdProveedorNoFormalizado);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MaterialesNoFormalizados",
                columns: table => new
                {
                    IdMaterialNoFormalizado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IdProveedorNoFormalizado = table.Column<int>(type: "int", nullable: false),
                    NombreMaterial = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DescripcionCompra = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Densidad = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Categoria = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoProduccion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unidad = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecioBaseKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DocumentoPdf = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialesNoFormalizados", x => x.IdMaterialNoFormalizado);
                    table.ForeignKey(
                        name: "FK_MaterialesNoFormalizados_ProveedoresNoFormalizados_IdProveed~",
                        column: x => x.IdProveedorNoFormalizado,
                        principalTable: "ProveedoresNoFormalizados",
                        principalColumn: "IdProveedorNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenesCompraNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProveedorNoFormalizadoId = table.Column<int>(type: "int", nullable: false),
                    FechaOrden = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FormaPago = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LugarEntrega = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    TotalKg = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalBultos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TipoImpuesto = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PorcentajeImpuesto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ValorImpuesto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TotalPagar = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioActualizacionId = table.Column<int>(type: "int", nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CorreoEnviado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaEnvioCorreo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioEnvioCorreoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraNoFormalizadas_ProveedoresNoFormalizados_Provee~",
                        column: x => x.ProveedorNoFormalizadoId,
                        principalTable: "ProveedoresNoFormalizados",
                        principalColumn: "IdProveedorNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraNoFormalizadas_Usuarios_UsuarioActualizacionId",
                        column: x => x.UsuarioActualizacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraNoFormalizadas_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraNoFormalizadas_Usuarios_UsuarioEnvioCorreoId",
                        column: x => x.UsuarioEnvioCorreoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesCorteInventarioNoFormalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CorteInventarioId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StockSistema = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ConteoFisico = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesCorteInventarioNoFormalizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesCorteInventarioNoFormalizados_CortesInventarioNoForm~",
                        column: x => x.CorteInventarioId,
                        principalTable: "CortesInventarioNoFormalizados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesCorteInventarioNoFormalizados_MaterialesNoFormalizad~",
                        column: x => x.MaterialId,
                        principalTable: "MaterialesNoFormalizados",
                        principalColumn: "IdMaterialNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InventariosNoFormalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StockActual = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CostoPromedio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ValorInventario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventariosNoFormalizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventariosNoFormalizados_MaterialesNoFormalizados_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "MaterialesNoFormalizados",
                        principalColumn: "IdMaterialNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenesTrasladoDetalleNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdenTrasladoNoFormalizadaId = table.Column<int>(type: "int", nullable: false),
                    MaterialNoFormalizadoId = table.Column<int>(type: "int", nullable: true),
                    Proveedor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Densidad = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CantidadKg = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Bultos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CantidadVerificadaKg = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BultosVerificados = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    EstadoVerificacion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesTrasladoDetalleNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesTrasladoDetalleNoFormalizadas_MaterialesNoFormalizado~",
                        column: x => x.MaterialNoFormalizadoId,
                        principalTable: "MaterialesNoFormalizados",
                        principalColumn: "IdMaterialNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesTrasladoDetalleNoFormalizadas_OrdenesTrasladoNoFormal~",
                        column: x => x.OrdenTrasladoNoFormalizadaId,
                        principalTable: "OrdenesTrasladoNoFormalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdenesCompraDetalleNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdenCompraNoFormalizadaId = table.Column<int>(type: "int", nullable: false),
                    MaterialNoFormalizadoId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CantidadKg = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    KgPorBulto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Bultos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CostoKg = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesCompraDetalleNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalleNoFormalizadas_MaterialesNoFormalizados_~",
                        column: x => x.MaterialNoFormalizadoId,
                        principalTable: "MaterialesNoFormalizados",
                        principalColumn: "IdMaterialNoFormalizado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenesCompraDetalleNoFormalizadas_OrdenesCompraNoFormalizad~",
                        column: x => x.OrdenCompraNoFormalizadaId,
                        principalTable: "OrdenesCompraNoFormalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecepcionesMercanciasNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroRecepcion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrdenCompraNoFormalizadaId = table.Column<int>(type: "int", nullable: false),
                    Conductor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Transportadora = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoDocumento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmbalajeAdecuado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Recibe = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cargo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaConfirmacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioConfirmacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesMercanciasNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercanciasNoFormalizadas_OrdenesCompraNoFormaliza~",
                        column: x => x.OrdenCompraNoFormalizadaId,
                        principalTable: "OrdenesCompraNoFormalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercanciasNoFormalizadas_Usuarios_UsuarioConfirma~",
                        column: x => x.UsuarioConfirmacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AjustesInventarioNoFormalizados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroAjuste = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InventarioId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cantidad = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    StockAnterior = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    StockNuevo = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CostoPromedio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AjustesInventarioNoFormalizados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AjustesInventarioNoFormalizados_InventariosNoFormalizados_In~",
                        column: x => x.InventarioId,
                        principalTable: "InventariosNoFormalizados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AjustesInventarioNoFormalizados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecepcionesMercanciaDetalleNoFormalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RecepcionMercanciaNoFormalizadaId = table.Column<int>(type: "int", nullable: false),
                    OrdenCompraDetalleNoFormalizadaId = table.Column<int>(type: "int", nullable: false),
                    CantidadRecibida = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BultosRecibidos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    LoteProveedor = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstadoMaterial = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecepcionesMercanciaDetalleNoFormalizadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercanciaDetalleNoFormalizadas_OrdenesCompraDetal~",
                        column: x => x.OrdenCompraDetalleNoFormalizadaId,
                        principalTable: "OrdenesCompraDetalleNoFormalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecepcionesMercanciaDetalleNoFormalizadas_RecepcionesMercanc~",
                        column: x => x.RecepcionMercanciaNoFormalizadaId,
                        principalTable: "RecepcionesMercanciasNoFormalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventarioNoFormalizados_InventarioId",
                table: "AjustesInventarioNoFormalizados",
                column: "InventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AjustesInventarioNoFormalizados_UsuarioId",
                table: "AjustesInventarioNoFormalizados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCorteInventarioNoFormalizados_CorteInventarioId",
                table: "DetallesCorteInventarioNoFormalizados",
                column: "CorteInventarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesCorteInventarioNoFormalizados_MaterialId",
                table: "DetallesCorteInventarioNoFormalizados",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_InventariosNoFormalizados_MaterialId_Color",
                table: "InventariosNoFormalizados",
                columns: new[] { "MaterialId", "Color" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialesNoFormalizados_IdProveedorNoFormalizado",
                table: "MaterialesNoFormalizados",
                column: "IdProveedorNoFormalizado");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalleNoFormalizadas_MaterialNoFormalizadoId",
                table: "OrdenesCompraDetalleNoFormalizadas",
                column: "MaterialNoFormalizadoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraDetalleNoFormalizadas_OrdenCompraNoFormalizadaId",
                table: "OrdenesCompraDetalleNoFormalizadas",
                column: "OrdenCompraNoFormalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraNoFormalizadas_ProveedorNoFormalizadoId",
                table: "OrdenesCompraNoFormalizadas",
                column: "ProveedorNoFormalizadoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraNoFormalizadas_UsuarioActualizacionId",
                table: "OrdenesCompraNoFormalizadas",
                column: "UsuarioActualizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraNoFormalizadas_UsuarioCreacionId",
                table: "OrdenesCompraNoFormalizadas",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesCompraNoFormalizadas_UsuarioEnvioCorreoId",
                table: "OrdenesCompraNoFormalizadas",
                column: "UsuarioEnvioCorreoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrasladoDetalleNoFormalizadas_MaterialNoFormalizadoId",
                table: "OrdenesTrasladoDetalleNoFormalizadas",
                column: "MaterialNoFormalizadoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrasladoDetalleNoFormalizadas_OrdenTrasladoNoFormaliz~",
                table: "OrdenesTrasladoDetalleNoFormalizadas",
                column: "OrdenTrasladoNoFormalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrasladoNoFormalizadas_UsuarioConfirmacionId",
                table: "OrdenesTrasladoNoFormalizadas",
                column: "UsuarioConfirmacionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrasladoNoFormalizadas_UsuarioId",
                table: "OrdenesTrasladoNoFormalizadas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrasladoNoFormalizadas_UsuarioVerificacionId",
                table: "OrdenesTrasladoNoFormalizadas",
                column: "UsuarioVerificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercanciaDetalleNoFormalizadas_OrdenCompraDetalle~",
                table: "RecepcionesMercanciaDetalleNoFormalizadas",
                column: "OrdenCompraDetalleNoFormalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercanciaDetalleNoFormalizadas_RecepcionMercancia~",
                table: "RecepcionesMercanciaDetalleNoFormalizadas",
                column: "RecepcionMercanciaNoFormalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercanciasNoFormalizadas_OrdenCompraNoFormalizada~",
                table: "RecepcionesMercanciasNoFormalizadas",
                column: "OrdenCompraNoFormalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_RecepcionesMercanciasNoFormalizadas_UsuarioConfirmacionId",
                table: "RecepcionesMercanciasNoFormalizadas",
                column: "UsuarioConfirmacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AjustesInventarioNoFormalizados");

            migrationBuilder.DropTable(
                name: "DetallesCorteInventarioNoFormalizados");

            migrationBuilder.DropTable(
                name: "OrdenesTrasladoDetalleNoFormalizadas");

            migrationBuilder.DropTable(
                name: "RecepcionesMercanciaDetalleNoFormalizadas");

            migrationBuilder.DropTable(
                name: "InventariosNoFormalizados");

            migrationBuilder.DropTable(
                name: "CortesInventarioNoFormalizados");

            migrationBuilder.DropTable(
                name: "OrdenesTrasladoNoFormalizadas");

            migrationBuilder.DropTable(
                name: "OrdenesCompraDetalleNoFormalizadas");

            migrationBuilder.DropTable(
                name: "RecepcionesMercanciasNoFormalizadas");

            migrationBuilder.DropTable(
                name: "MaterialesNoFormalizados");

            migrationBuilder.DropTable(
                name: "OrdenesCompraNoFormalizadas");

            migrationBuilder.DropTable(
                name: "ProveedoresNoFormalizados");
        }
    }
}
