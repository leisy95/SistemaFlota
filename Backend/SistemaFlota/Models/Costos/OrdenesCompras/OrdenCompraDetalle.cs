using SistemaFlota.Models.Prov_Materiales.Materiales;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Costos.OrdenesCompras
{
    public class OrdenCompraDetalle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrdenCompraId { get; set; }

        [ForeignKey(nameof(OrdenCompraId))]
        public virtual OrdenCompra? OrdenCompra { get; set; }

        [Required]
        public int MaterialId { get; set; }

        [ForeignKey(nameof(MaterialId))]
        public virtual Material? Material { get; set; }

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