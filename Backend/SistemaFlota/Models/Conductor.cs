using System.ComponentModel.DataAnnotations;

namespace SistemaFlota
{
    public class Conductor
    {
        [Key] // 👈 MUY IMPORTANTE
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Licencia { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Foto { get; set; }
    }
}