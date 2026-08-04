using System.ComponentModel.DataAnnotations;
namespace SistemaFlota.Models
{
    public class ConversacionFlotaChat
    {
        [Key] public int Id { get; set; }
        [Required] public int FlotaChatUsuarioId { get; set; }
        [Required][MaxLength(50)] public string Paso { get; set; } = string.Empty; // EsperandoConfirmacion | EsperandoPlaca
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime FechaExpiracion { get; set; } = DateTime.Now.AddMinutes(10);
    }
}