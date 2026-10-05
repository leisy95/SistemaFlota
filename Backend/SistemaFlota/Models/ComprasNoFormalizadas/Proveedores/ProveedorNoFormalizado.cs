using SistemaFlota.Models.ComprasNoFormalizadas.Materiales;
using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.ComprasNoFormalizadas.Proveedores
{
    public class ProveedorNoFormalizado
    {
        [Key]
        public int IdProveedorNoFormalizado { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Documento { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? CorreoElectronico { get; set; }

        public string? Direccion { get; set; }

        public string? Ciudad { get; set; }

        public string? Departamento { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        public virtual ICollection<MaterialNoFormalizado> Materiales { get; set; }
            = new List<MaterialNoFormalizado>();
    }
}