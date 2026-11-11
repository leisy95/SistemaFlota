using SistemaFlota.Models.InventariosBodega.SalidasBodega;
using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.InventariosBodega.Clientes
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Direccion { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }
        public ICollection<SalidaBodega> SalidasBodega { get; set; }
            = new List<SalidaBodega>();
    }
}
