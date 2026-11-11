using SistemaFlota.Models.InventariosBodega.Inventario;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.InventariosBodega.EntradasBodega
{
    public class InventarioBodegaItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // Relación con InventarioBodega
        [Required]
        public int IdInventarioBodega { get; set; }

        [ForeignKey(nameof(IdInventarioBodega))]
        public InventarioBodega InventarioBodega { get; set; }

        [StringLength(200)]
        public string ReferenciaPeso { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PesoActual { get; set; } = 0;

        [Required]
        public DateTime FechaRegistroItem { get; set; }

        [StringLength(50)]
        public string Estado { get; set; }

        // Relación con movimientos
        public ICollection<MovimientoInventarioBodega> Movimientos { get; set; }
            = new List<MovimientoInventarioBodega>();
    }
}
