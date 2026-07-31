using Microsoft.EntityFrameworkCore;
using SistemaFlota.Models;
using SistemaFlota.Models.Consecutivo;
using SistemaFlota.Models.Costos.OrdenesCompras;
using SistemaFlota.Models.Costos.RecepcionMercancia;
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

        // ── HOJA DE VIDA ──────────────────────────────────────────────────────
        public DbSet<ExamenMedico> ExamenesMedicos { get; set; }
        public DbSet<Capacitacion> Capacitaciones { get; set; }
        public DbSet<InfraccionConductor> Infracciones { get; set; }

        // ── PEDIDOS ───────────────────────────────────────────────────────────
        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<PedidoReferencia> PedidoReferencias { get; set; }

        public DbSet<SeguimientoRrhh> SeguimientosRrhh { get; set; }
        public DbSet<SeguimientoRrhhFoto> SeguimientosRrhhFotos { get; set; }

        public DbSet<Cajon> Cajones { get; set; }
        public DbSet<CyreleRegistro> CyreleRegistros { get; set; }
        public DbSet<InventarioRuta> InventarioRutas { get; set; }
        public DbSet<FormatoFGC008> FormatosFGC008 { get; set; }
        public DbSet<NumeroEmergencia> NumerosEmergencia { get; set; }
        public DbSet<CosteFlete> CostosFletes { get; set; }

        //  --Costos--
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Material> Materiales { get; set; }
        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<OrdenCompraDetalle> OrdenesCompraDetalle { get; set; }
        public DbSet<RecepcionMercancia> RecepcionesMercancia { get; set; }
        public DbSet<RecepcionMercanciaDetalle> RecepcionesMercanciaDetalle { get; set; }
        public DbSet<Consecutivo> Consecutivos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                .WithMany()
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

            modelBuilder.Entity<Consecutivo>()
                .HasIndex(x => x.Modulo)
                .IsUnique();
        }
    }
}