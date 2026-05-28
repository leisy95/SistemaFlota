using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class DocumentoVehiculo
    {
        [Key]
        public int Id { get; set; }

        public int VehiculoId { get; set; }
        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        // TIPO
        // SOAT | Tecnomecanica | TarjetaPropiedad | Otro
        public string TipoDocumento { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string ArchivoUrl { get; set; } = string.Empty;
        public string? Extension { get; set; }

        public DateTime FechaSubida { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        public bool Activo { get; set; } = true;
    }
}