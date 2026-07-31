using SistemaFlota.Models.Prov_Materiales.Materiales;
using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.Proveedores
{
    public class Proveedor
    {
        [Key]
        public int IdProveedor { get; set; }

        [Required]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Nit { get; set; } = string.Empty;

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

        // Relación (cuando crees Material)
        public virtual ICollection<Material> Materiales { get; set; } = new List<Material>();
    }
}
