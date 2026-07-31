using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Costos.Materiales
{
    public class CrearMaterialDto
    {
        [Required]
        public int IdProveedor { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreMaterial { get; set; } = string.Empty;

        [StringLength(250)]
        public string DescripcionCompra { get; set; } = string.Empty;

        [StringLength(50)]
        public string Densidad { get; set; } = string.Empty;

        [StringLength(50)]
        public string Categoria { get; set; } = string.Empty;

        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [StringLength(50)]
        public string TipoProduccion { get; set; } = string.Empty;

        public decimal PrecioBaseKg { get; set; }

        [StringLength(20)]
        public string Unidad { get; set; } = string.Empty;

        public bool Activo { get; set; }
        public IFormFile? ArchivoPdf { get; set; }
    }
}
