using System.ComponentModel.DataAnnotations;
namespace SistemaFlota.Models
{
    public class OrdenProduccionExterna
    {
        [Key] public int Id { get; set; }
        [Required][MaxLength(50)] public string NumeroOP { get; set; } = string.Empty;
        [MaxLength(200)] public string? Cliente { get; set; }
        public int CantidadOP { get; set; }
        [MaxLength(200)] public string? Referencia { get; set; }
        public DateTime FechaImportacion { get; set; } = DateTime.Now;
    }
}