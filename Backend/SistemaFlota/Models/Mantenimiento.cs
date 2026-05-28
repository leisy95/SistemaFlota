using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class Mantenimiento
    {
        [Key]
        public int Id { get; set; }

        // VEHÍCULO
        public int VehiculoId { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // TIPO
        // Preventivo | Correctivo | Predictivo
        public string TipoMantenimiento { get; set; } = string.Empty;

        // FECHAS
        public DateTime FechaEntrada { get; set; } = DateTime.Now;
        public DateTime? FechaSalida { get; set; }

        // KILOMETRAJE
        public int KilometrajeEntrada { get; set; }
        public int? KilometrajeSiguiente { get; set; }
        public DateTime? FechaSiguiente { get; set; }

        // TALLER
        public string NombreTaller { get; set; } = string.Empty;
        public string? TecnicoResponsable { get; set; }
        public string? TelefonoTaller { get; set; }

        // TRABAJOS Y REPUESTOS
        public string TrabajosRealizados { get; set; } = string.Empty;
        public string? RepuestosUtilizados { get; set; }

        // COSTOS
        public decimal CostoManoObra { get; set; }
        public decimal CostoRepuestos { get; set; }
        public decimal CostoTotal { get; set; }

        // ESTADO
        // EnTaller | Finalizado | Cancelado
        public string Estado { get; set; } = "EnTaller";

        // OBSERVACIONES
        public string? Observaciones { get; set; }

        // FOTOS
        public string? Fotos { get; set; }
    }
}