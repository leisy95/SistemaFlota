using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
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
    }
}