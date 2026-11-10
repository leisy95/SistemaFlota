
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.InventarioBodega
{
    public class MovimientoInventarioBodega
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Referencia { get; set; }

        [Required]
        [MaxLength(60)]
        public string ReferenciaPeso { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Peso { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(100)]
        public string Tipo { get; set; }

        [MaxLength(100)]
        public string Usuario { get; set; }

        [StringLength(500)]
        public string? Observacion { get; set; }

        // Relación con InventarioBodega
        [Required]
        public int IdInventarioBodega { get; set; }

        [ForeignKey(nameof(IdInventarioBodega))]
        public InventarioBodega InventarioBodega { get; set; }

        // Relación con InventarioBodegaItem
        [Required]
        public int IdInventarioBodegaItem { get; set; }

        [ForeignKey(nameof(IdInventarioBodegaItem))]
        public InventarioBodegaItem InventarioBodegaItem { get; set; }
    }
}
