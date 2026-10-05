using SistemaFlota.Models.ComprasNoFormalizadas.Proveedores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.ComprasNoFormalizadas.Materiales
{
    public class MaterialNoFormalizado
    {
        [Key]
        public int IdMaterialNoFormalizado { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        public int IdProveedorNoFormalizado { get; set; }

        [ForeignKey(nameof(IdProveedorNoFormalizado))]
        public ProveedorNoFormalizado? ProveedorNoFormalizado { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreMaterial { get; set; } = string.Empty;

        [StringLength(250)]
        public string? DescripcionCompra { get; set; }

        [Required]
        [StringLength(50)]
        public string Densidad { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? TipoProduccion { get; set; }

        [Required]
        [StringLength(20)]
        public string Unidad { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBaseKg { get; set; }

        public bool Activo { get; set; } = true;

        public string? DocumentoPdf { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }
    }
}