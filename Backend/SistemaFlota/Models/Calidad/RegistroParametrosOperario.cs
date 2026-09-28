using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Calidad
{
    public class RegistroParametrosOperario
    {
        [Key] public int Id { get; set; }
        [Required] public int RegistroFormatoCalidadId { get; set; }
        [Required][MaxLength(200)] public string OperarioNombre { get; set; } = string.Empty;
        public string? VariablesCriticasJson { get; set; }
        public DateTime FechaGuardado { get; set; } = DateTime.Now;
        public string? MotivoCambio { get; set; }

        [ForeignKey("RegistroFormatoCalidadId")]
        public RegistroFormatoCalidad? RegistroFormatoCalidad { get; set; }
    }
}