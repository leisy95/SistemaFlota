using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class CambioRuta
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        // ========================
        // VINCULACIÓN OPCIONAL
        // ========================
        public int? AutorizacionId { get; set; }
        [ForeignKey("AutorizacionId")]
        public Autorizacion? Autorizacion { get; set; }

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
        // RUTAS
        // ========================
        public string RutaOriginal { get; set; } = string.Empty;
        public string NuevaRuta { get; set; } = string.Empty;
        public string MotivoCambio { get; set; } = string.Empty;

        // ========================
        // ESTADO
        // Pendiente | Autorizado | Rechazado
        // ========================
        public string Estado { get; set; } = "Pendiente";

        // ========================
        // AUTORIZACIÓN
        // ========================
        public string? AutorizadoPor { get; set; }
        public string? ObservacionAut { get; set; }
        public DateTime? FechaAutorizacion { get; set; }
    }
}