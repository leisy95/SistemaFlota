using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class Inspeccion
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public int Kilometraje { get; set; }

        public string? FotoOdometro { get; set; }

        // FIRMA CONDUCTOR
        public string? FirmaCondutor { get; set; }

        // VEHÍCULO
        public int VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // CONDUCTOR
        public int ConductorId { get; set; }

        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        // DETALLES
        public List<InspeccionDetalle> Detalles { get; set; } = new();
    }
}