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
        public string MateriaPrima { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Densidad { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Calidad { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? LineaProduccion { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioBaseKg { get; set; }

        public int Bultos { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CantidadKg { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }
    }
}
