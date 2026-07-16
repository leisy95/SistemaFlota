using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Prov_Materiales.Proveedores
{
    public class ActualizarProveedorDto
    {
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

        [EmailAddress]
        [StringLength(150)]
        public string? CorreoElectronico { get; set; }

        public bool Activo { get; set; }
    }
}
