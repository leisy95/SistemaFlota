using System.ComponentModel.DataAnnotations;
namespace SistemaFlota.Models
{
    public class TipoFormatoCalidad
    {
        [Key] public int Id { get; set; }
        [Required][MaxLength(20)] public string Codigo { get; set; } = string.Empty; // F-GC-004, F-GC-005, etc.
        [Required][MaxLength(100)] public string Nombre { get; set; } = string.Empty; // Extrusión, Impresión, etc.
        public bool TieneVariablesCriticas { get; set; } = false;
        public ICollection<CaracteristicaFormato> Caracteristicas { get; set; } = new List<CaracteristicaFormato>();
    }
}