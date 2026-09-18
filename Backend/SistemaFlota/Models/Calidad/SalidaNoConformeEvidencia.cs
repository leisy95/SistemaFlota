using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Calidad
{
    public class SalidaNoConformeEvidencia
    {
        [Key] public int Id { get; set; }
        public int SalidaNoConformeId { get; set; }
        [MaxLength(30)] public string Paso { get; set; } = string.Empty; // Reportar | Tratamiento | Verificacion
        [MaxLength(300)] public string NombreArchivo { get; set; } = string.Empty;
        [MaxLength(20)] public string TipoArchivo { get; set; } = string.Empty; // Imagen | Pdf
        public DateTime FechaSubida { get; set; } = DateTime.Now;

        [ForeignKey("SalidaNoConformeId")]
        public SalidaNoConforme? SalidaNoConforme { get; set; }
    }
}