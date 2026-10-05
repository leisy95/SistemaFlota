using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Material
{
    public class CrearMaterialNoFormalizadoDto
    {
        [Required]
        public int IdProveedorNoFormalizado { get; set; }

        [Required]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

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

        public decimal PrecioBaseKg { get; set; }

        public bool Activo { get; set; } = true;

        public IFormFile? ArchivoPdf { get; set; }
    }
}
