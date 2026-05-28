using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class SolicitudTaller
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

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
        // SOLICITUD
        // ========================
        public string TipoMantenimiento { get; set; } = string.Empty;
        public string DescripcionProblema { get; set; } = string.Empty;
        public string? FotoOdometro { get; set; }
        public int? Kilometraje { get; set; }

        // ========================
        // ESTADO
        // Pendiente | Autorizado | EnTaller | Finalizado | Rechazado
        // ========================
        public string Estado { get; set; } = "Pendiente";

        // ========================
        // AUTORIZACIÓN SALIDA
        // ========================
        public string? AutorizadoPor { get; set; }
        public string? ObservacionAut { get; set; }
        public DateTime? FechaAutorizacion { get; set; }

        // ========================
        // FACTURA TALLER
        // ========================
        public string? NumeroFacturaTaller { get; set; }
        public decimal? ValorFactura { get; set; }
        public DateTime? FechaFactura { get; set; }
        public bool FacturaValidada { get; set; } = false;
        public string? ObservacionFactura { get; set; }
    }
}