using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Costos.Materiales
{
    public class ActualizarMaterialDto
    {
        [Required]
        public int IdProveedor { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreMaterial{ get; set; } = string.Empty;

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

        [Range(0, double.MaxValue)]
        public decimal PrecioBaseKg { get; set; }

        public bool Activo { get; set; }
        public IFormFile? ArchivoPdf { get; set; }
    }
}
