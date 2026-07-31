using SistemaFlota.Models.Proveedores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Prov_Materiales.Materiales
{
    public class Material
    {
        [Key]
        public int IdMaterial { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        [ForeignKey(nameof(IdProveedor))]
        public Proveedor? Proveedor { get; set; }

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

        [Column(TypeName = "decimal(18,2)")]

        public bool Activo { get; set; } = true;

        public string? DocumentoPdf { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }
    }
}
