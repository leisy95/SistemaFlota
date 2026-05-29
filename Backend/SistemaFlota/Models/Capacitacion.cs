using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota
{
    public class Capacitacion
    {
        [Key]
        public int Id { get; set; }

        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        public string Nombre { get; set; } = string.Empty;
        // Seguridad vial | Primeros auxilios | Manejo defensivo | Mercancías peligrosas | Fatiga | Otro
        public string Tipo { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public string? Instructor { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? DuracionHoras { get; set; }
        public bool Aprobado { get; set; } = true;
        public string? Observaciones { get; set; }
        public string? Documento { get; set; } // certificado PDF

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
