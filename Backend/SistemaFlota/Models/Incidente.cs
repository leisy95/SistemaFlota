using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class Incidente
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaReporte { get; set; } = DateTime.Now;

        // CONDUCTOR
        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        // VEHÍCULO
        public int VehiculoId { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // AUTORIZACIÓN RELACIONADA
        public int? AutorizacionId { get; set; }
        [ForeignKey("AutorizacionId")]
        public Autorizacion? Autorizacion { get; set; }

        // TIPO DE INCIDENTE
        // DañoMecanico | Averia | Trancon | CierreVia | Accidente | Otro
        public string TipoIncidente { get; set; } = string.Empty;

        // DESCRIPCIÓN
        public string DescripcionDetallada { get; set; } = string.Empty;

        // UBICACIÓN GPS
        public string? UbicacionGPS { get; set; }
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }

        // FOTOS (máximo 5, separadas por coma)
        public string? Fotos { get; set; }

        // ESTADO
        // Pendiente | Revisado
        public string Estado { get; set; } = "Pendiente";

        // REVISIÓN
        public DateTime? FechaRevision { get; set; }
        public string? RevisadoPor { get; set; }
        public string? ObservacionRevision { get; set; }
    }
}