using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.Categorias
{
    public class Categoria
    {
        [Key]
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
