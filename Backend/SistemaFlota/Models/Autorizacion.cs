using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class Autorizacion
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Pendiente | Bodega | Porteria | Autorizado | Rechazado
        public string Estado { get; set; } = "Pendiente";

        // ========================
        // CONDUCTOR Y VEHÍCULO
        // ========================
        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        public int VehiculoId { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // ========================
        // DETALLES DE SALIDA
        // ========================
        public string DestinoCompleto { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public decimal PesoKilos { get; set; }
        public string TipoVuelta { get; set; } = string.Empty;
        public string DescripcionCarga { get; set; } = string.Empty;
        public string? NumeroGuia { get; set; }

        // ========================
        // FACTURAS Y CLIENTES
        // JSON: [{"facturaRemision":"F-001","cliente":"Juan García"}, ...]
        // ========================
        public string? FacturasClientes { get; set; }

        // ========================
        // FIRMA FACTURACIÓN
        // ========================
        public string? FirmaFacturacion { get; set; }
        public DateTime? FechaFacturacion { get; set; }
        public string? UsuarioFacturacion { get; set; }
        public string? ObservacionFacturacion { get; set; }

        // ========================
        // FIRMA BODEGA
        // ========================
        public string? FirmaBodega { get; set; }
        public DateTime? FechaBodega { get; set; }
        public string? UsuarioBodega { get; set; }
        public string? ObservacionBodega { get; set; }

        // ========================
        // FIRMA PORTERÍA
        // ========================
        public string? FirmaPorteria { get; set; }
        public DateTime? FechaPorteria { get; set; }
        public string? UsuarioPorteria { get; set; }
        public string? ObservacionPorteria { get; set; }
    }
}