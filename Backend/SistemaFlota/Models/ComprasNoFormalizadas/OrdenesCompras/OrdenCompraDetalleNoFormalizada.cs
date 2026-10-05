using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.ComprasNoFormalizadas.OrdenesCompras
{
    public class OrdenCompraDetalleNoFormalizada
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenCompraNoFormalizadaId { get; set; }

        [ForeignKey(nameof(OrdenCompraNoFormalizadaId))]
        public virtual OrdenCompraNoFormalizada? OrdenCompraNoFormalizada { get; set; }

        [Required]
        public int MaterialNoFormalizadoId { get; set; }

        [ForeignKey(nameof(MaterialNoFormalizadoId))]
        public virtual MaterialNoFormalizado? MaterialNoFormalizado { get; set; }

        [Required]
        [MaxLength(100)]
        public string Color { get; set; } = string.Empty;

        public decimal CantidadKg { get; set; }

        public decimal KgPorBulto { get; set; }

        public decimal Bultos { get; set; }

        public decimal CostoKg { get; set; }

        public decimal Subtotal { get; set; }
    }
}