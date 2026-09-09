using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.Colores
{
    public class Color
    {
        [Key]
        public int IdColor { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
