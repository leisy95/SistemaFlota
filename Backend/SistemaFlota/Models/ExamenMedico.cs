using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota
{
    public class ExamenMedico
    {
        [Key]
        public int Id { get; set; }

        public int ConductorId { get; set; }
        [ForeignKey("ConductorId")]
        public Conductor? Conductor { get; set; }

        // Preingreso | Periódico | Retiro | Reintegro | Post-incapacidad
        public string TipoExamen { get; set; } = string.Empty;
        public DateTime FechaExamen { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }

        // Apto | Apto con restricciones | No apto
        public string Resultado { get; set; } = string.Empty;
        public string? Observaciones { get; set; }
        public string? Medico { get; set; }
        public string? Entidad { get; set; }
        public string? Documento { get; set; } // nombre del archivo PDF

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}