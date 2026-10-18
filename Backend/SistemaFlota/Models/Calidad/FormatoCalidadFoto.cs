using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Calidad
{
    [Table("FormatoCalidadFoto")]
    public class FormatoCalidadFoto
    {
        [Key] public int Id { get; set; }

        public int RegistroFormatoCalidadId { get; set; }

        // Identificador estable de la hora dentro del registro
        [Required][MaxLength(50)] public string RondaId { get; set; } = string.Empty;

        [Required][MaxLength(100)] public string Archivo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [MaxLength(100)] public string? SubidaPor { get; set; }

        [ForeignKey("RegistroFormatoCalidadId")]
        public RegistroFormatoCalidad? RegistroFormatoCalidad { get; set; }
    }
}
