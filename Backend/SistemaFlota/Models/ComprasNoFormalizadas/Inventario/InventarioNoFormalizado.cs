using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.ComprasNoFormalizadas.Inventario
{
    public class InventarioNoFormalizado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public virtual MaterialNoFormalizado? Material { get; set; }

        public string Color { get; set; } = string.Empty;

        public decimal StockActual { get; set; }

        public decimal CostoPromedio { get; set; }

        public decimal ValorInventario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime FechaActualizacion { get; set; } = DateTime.Now;

        public ICollection<AjusteInventarioNoFormalizado> AjustesInventario { get; set; }
            = new List<AjusteInventarioNoFormalizado>();
    }
}
