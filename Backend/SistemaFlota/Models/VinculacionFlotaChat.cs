using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class VinculacionFlotaChat
    {
        [Key]
        public int Id { get; set; }
        public int FlotaChatUsuarioId { get; set; }
        public string TipoEntidad { get; set; } = string.Empty; // "Conductor", "Cliente", "Taller"
        public int EntidadId { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaVinculacion { get; set; } = DateTime.Now;
    }
}