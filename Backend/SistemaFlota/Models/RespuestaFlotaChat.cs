using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace SistemaFlota
{
    [Index(nameof(IdEvento), IsUnique = true)]
    public class RespuestaFlotaChat
    {
        [Key]
        public int Id { get; set; }
        public int FlotaChatUsuarioId { get; set; }
        public int? ConductorId { get; set; }
        public int GrupoId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaRecibido { get; set; } = DateTime.Now;
        public bool Procesado { get; set; } = false;

        
        [MaxLength(64)] public string? IdEvento { get; set; }
        [MaxLength(20)] public string? TipoMensaje { get; set; }
        public int? MensajeId { get; set; }
    }
}