using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;
using SistemaFlota.Models.Calidad;
using SistemaFlota.Models.Categorias;
using SistemaFlota.Models.Colores;
using SistemaFlota.Models.ComprasNoFormalizadas.Inventario;
using SistemaFlota.Models.ComprasNoFormalizadas.Inventario.CortesInventario;
using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;
using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesCompras;
using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesTraslado;
using SistemaFlota.Models.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias;
using SistemaFlota.Models.Consecutivo;
using SistemaFlota.Models.Costos.Inventario;
using SistemaFlota.Models.Costos.Inventario.CortesInventario;
using SistemaFlota.Models.Costos.OrdenesCompras;
using SistemaFlota.Models.Costos.OrdenesTraslado;
using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Models.HistorialVersionOdo;
using SistemaFlota.Models.Idempotencia;
using SistemaFlota.Models.Prov_Materiales.Materiales;
using SistemaFlota.Models.Proveedores;


namespace SistemaFlota
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Conductor> Conductores { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoVehiculo> TiposVehiculo { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }
        public DbSet<Inspeccion> Inspecciones { get; set; }
        public DbSet<InspeccionDetalle> InspeccionDetalles { get; set; }
        public DbSet<Autorizacion> Autorizaciones { get; set; }
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }
        public DbSet<Incidente> Incidentes { get; set; }
        public DbSet<ContactoNotificacion> ContactosNotificacion { get; set; }
        public DbSet<ConfiguracionEmpresa> ConfiguracionEmpresa { get; set; }
        public DbSet<Mantenimiento> Mantenimientos { get; set; }
        public DbSet<DocumentoVehiculo> DocumentosVehiculo { get; set; }
        public DbSet<DocumentoGeneral> DocumentosGenerales { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<EncuestaFatiga> EncuestasFatiga { get; set; }
        public DbSet<TrazabilidadFactura> TrazabilidadFacturas { get; set; }
        public DbSet<NotaTrazabilidad> NotasTrazabilidad { get; set; }
        public DbSet<CambioRuta> CambiosRuta { get; set; }
        public DbSet<SolicitudTaller> SolicitudesTaller { get; set; }
        public DbSet<ExamenMedico> ExamenesMedicos { get; set; }
        public DbSet<Capacitacion> Capacitaciones { get; set; }
        public DbSet<InfraccionConductor> Infracciones { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoReferencia> PedidoReferencias { get; set; }
        public DbSet<SeguimientoRrhh> SeguimientosRrhh { get; set; }
        public DbSet<SeguimientoRrhhFoto> SeguimientosRrhhFotos { get; set; }
        public DbSet<Cajon> Cajones { get; set; }
        public DbSet<CyreleRegistro> CyreleRegistros { get; set; }
        public DbSet<CyreleFoto> CyreleFotos { get; set; }
        public DbSet<InventarioRuta> InventarioRutas { get; set; }
        public DbSet<FormatoFGC008> FormatosFGC008 { get; set; }
        public DbSet<NumeroEmergencia> NumerosEmergencia { get; set; }
        public DbSet<CosteFlete> CostosFletes { get; set; }
        public DbSet<VinculacionFlotaChat> VinculacionesFlotaChat { get; set; }
        public DbSet<RespuestaFlotaChat> RespuestasFlotaChat { get; set; }
        public DbSet<OrdenProduccionExterna> OrdenesProduccionExternas { get; set; }
        public DbSet<TipoFormatoCalidad> TiposFormatoCalidad { get; set; }
        public DbSet<CaracteristicaFormato> CaracteristicasFormato { get; set; }
        public DbSet<RegistroFormatoCalidad> RegistrosFormatoCalidad { get; set; }
        public DbSet<RegistroParametrosOperario> RegistrosParametrosOperario { get; set; }
        public DbSet<OpcionFormulario> OpcionesFormulario { get; set; }
        public DbSet<ConversacionFlotaChat> ConversacionesFlotaChat { get; set; }

        // Idempotencia
        public DbSet<IdempotencyLog> IdempotencyLogs { get; set; }

        //  --Costos--
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle { get; set; }
        public DbSet<RecepcionMercancia> RecepcionesMercancias { get; set; }
        public DbSet<RecepcionMercanciaDetalle> RecepcionesMercanciaDetalle { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<AjusteInventario> AjustesInventario { get; set; }
        public DbSet<CorteInventario> CortesInventario { get; set; }
        public DbSet<OrdenTraslado> OrdenesTraslado { get; set; }
        public DbSet<OrdenTrasladoDetalle> OrdenesTrasladoDetalle { get; set; }
        public DbSet<Consecutivo> Consecutivos { get; set; }
        public DbSet<Color> Colores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<SalidaNoConforme> SalidasNoConforme { get; set; }
        public DbSet<SalidaNoConformeEvidencia> SalidasNoConformeEvidencias { get; set; }

        // -- Compras No Formalizadas --
        public DbSet<ProveedorNoFormalizado> ProveedoresNoFormalizados { get; set; }
        public DbSet<MaterialNoFormalizado> MaterialesNoFormalizados { get; set; }
        public DbSet<OrdenCompraNoFormalizada> OrdenesCompraNoFormalizadas { get; set; }
        public DbSet<OrdenCompraDetalleNoFormalizada> OrdenesCompraDetalleNoFormalizadas { get; set; }
        public DbSet<RecepcionMercanciaNoFormalizada> RecepcionesMercanciasNoFormalizadas { get; set; }
        public DbSet<RecepcionMercanciaDetalleNoFormalizada> RecepcionesMercanciaDetalleNoFormalizadas { get; set; }
        public DbSet<InventarioNoFormalizado> InventariosNoFormalizados { get; set; }
        public DbSet<AjusteInventarioNoFormalizado> AjustesInventarioNoFormalizados { get; set; }
        public DbSet<CorteInventarioNoFormalizado> CortesInventarioNoFormalizados { get; set; }
        public DbSet<DetalleCorteInventarioNoFormalizado> DetallesCorteInventarioNoFormalizados { get; set; }
        public DbSet<OrdenTrasladoNoFormalizada> OrdenesTrasladoNoFormalizadas { get; set; }
        public DbSet<OrdenTrasladoDetalleNoFormalizada> OrdenesTrasladoDetalleNoFormalizadas { get; set; }

        // Historial Version Odo
        public DbSet<PedidoCompra> PedidosCompra { get; set; }
        public DbSet<MovimientoProducto> MovimientosProducto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdempotencyLog>()
                .HasIndex(x => x.Key)
                .IsUnique();

            modelBuilder.Entity<Material>()
                .HasOne(m => m.Proveedor)
                .WithMany(p => p.Materiales)
                .HasForeignKey(m => m.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.Proveedor)
                .WithMany()
                .HasForeignKey(o => o.ProveedorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.UsuarioCreacion)
                .WithMany()
                .HasForeignKey(o => o.UsuarioCreacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne(o => o.UsuarioActualizacion)
                .WithMany()
                .HasForeignKey(o => o.UsuarioActualizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompra>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(o => o.UsuarioEnvioCorreoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraDetalle>()
                .HasOne(d => d.OrdenCompra)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenCompraId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenCompraDetalle>()
                .HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecepcionMercancia>()
                .HasOne(r => r.OrdenCompra)
                .WithMany(o => o.RecepcionesMercancia)
                .HasForeignKey(r => r.OrdenCompraId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecepcionMercanciaDetalle>()
                .HasOne(d => d.RecepcionMercancia)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.RecepcionMercanciaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecepcionMercanciaDetalle>()
                .HasOne(d => d.OrdenCompraDetalle)
                .WithMany()
                .HasForeignKey(d => d.OrdenCompraDetalleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Material)
                .WithMany()
                .HasForeignKey(i => i.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Inventario>()
                .HasIndex(i => new { i.MaterialId, i.Color })
                .IsUnique();

            modelBuilder.Entity<AjusteInventario>()
                .HasOne(a => a.Inventario)
                .WithMany(i => i.AjustesInventario)
                .HasForeignKey(a => a.InventarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AjusteInventario>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CorteInventario>()
                .HasMany(c => c.Detalles)
                .WithOne(d => d.CorteInventario)
                .HasForeignKey(d => d.CorteInventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleCorteInventario>()
                .HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTraslado>()
                .HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTraslado>()
                .HasOne(x => x.UsuarioVerificacion)
                .WithMany()
                .HasForeignKey(x => x.UsuarioVerificacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTraslado>()
                .HasOne(x => x.UsuarioConfirmacion)
                .WithMany()
                .HasForeignKey(x => x.UsuarioConfirmacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTraslado>()
                .HasMany(x => x.Detalles)
                .WithOne(x => x.OrdenTraslado)
                .HasForeignKey(x => x.OrdenTrasladoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenTrasladoDetalle>()
                .HasOne(x => x.Material)
                .WithMany()
                .HasForeignKey(x => x.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consecutivo>()
                .HasIndex(x => x.Modulo)
                .IsUnique();

            // Compras no formalizadas
            modelBuilder.Entity<MaterialNoFormalizado>()
                .HasOne(m => m.ProveedorNoFormalizado)
                .WithMany(p => p.Materiales)
                .HasForeignKey(m => m.IdProveedorNoFormalizado)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraNoFormalizada>()
                .HasOne(o => o.ProveedorNoFormalizado)
                .WithMany()
                .HasForeignKey(o => o.ProveedorNoFormalizadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraNoFormalizada>()
                .HasOne(o => o.UsuarioCreacion)
                .WithMany()
                .HasForeignKey(o => o.UsuarioCreacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraNoFormalizada>()
                .HasOne(o => o.UsuarioActualizacion)
                .WithMany()
                .HasForeignKey(o => o.UsuarioActualizacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraNoFormalizada>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(o => o.UsuarioEnvioCorreoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenCompraDetalleNoFormalizada>()
                .HasOne(d => d.OrdenCompraNoFormalizada)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenCompraNoFormalizadaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenCompraDetalleNoFormalizada>()
                .HasOne(d => d.MaterialNoFormalizado)
                .WithMany()
                .HasForeignKey(d => d.MaterialNoFormalizadoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecepcionMercanciaNoFormalizada>()
                .HasOne(r => r.OrdenCompraNoFormalizada)
                .WithMany(o => o.RecepcionesMercancia)
                .HasForeignKey(r => r.OrdenCompraNoFormalizadaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RecepcionMercanciaDetalleNoFormalizada>()
                .HasOne(d => d.RecepcionMercanciaNoFormalizada)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.RecepcionMercanciaNoFormalizadaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecepcionMercanciaDetalleNoFormalizada>()
                .HasOne(d => d.OrdenCompraDetalleNoFormalizada)
                .WithMany()
                .HasForeignKey(d => d.OrdenCompraDetalleNoFormalizadaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventario - Compras No Formalizadas
            modelBuilder.Entity<InventarioNoFormalizado>()
                .HasOne(i => i.Material)
                .WithMany()
                .HasForeignKey(i => i.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventarioNoFormalizado>()
                .HasIndex(i => new { i.MaterialId, i.Color })
                .IsUnique();

            modelBuilder.Entity<AjusteInventarioNoFormalizado>()
                .HasOne(a => a.Inventario)
                .WithMany(i => i.AjustesInventario)
                .HasForeignKey(a => a.InventarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AjusteInventarioNoFormalizado>()
                .HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CorteInventarioNoFormalizado>()
                .HasMany(c => c.Detalles)
                .WithOne(d => d.CorteInventario)
                .HasForeignKey(d => d.CorteInventarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleCorteInventarioNoFormalizado>()
                .HasOne(d => d.Material)
                .WithMany()
                .HasForeignKey(d => d.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // Traslados - Compras No Formalizadas
            modelBuilder.Entity<OrdenTrasladoNoFormalizada>()
                .HasOne(x => x.Usuario)
                .WithMany()
                .HasForeignKey(x => x.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrasladoNoFormalizada>()
                .HasOne(x => x.UsuarioVerificacion)
                .WithMany()
                .HasForeignKey(x => x.UsuarioVerificacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrasladoNoFormalizada>()
                .HasOne(x => x.UsuarioConfirmacion)
                .WithMany()
                .HasForeignKey(x => x.UsuarioConfirmacionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrdenTrasladoNoFormalizada>()
                .HasMany(x => x.Detalles)
                .WithOne(x => x.OrdenTrasladoNoFormalizada)
                .HasForeignKey(x => x.OrdenTrasladoNoFormalizadaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrdenTrasladoDetalleNoFormalizada>()
                .HasOne(x => x.MaterialNoFormalizado)
                .WithMany()
                .HasForeignKey(x => x.MaterialNoFormalizadoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
