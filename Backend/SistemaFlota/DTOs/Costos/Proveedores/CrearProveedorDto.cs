using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.Prov_Materiales.Proveedores
{
    public class CrearProveedorDto
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

        public string? Direccion { get; set; }

        public string? Ciudad { get; set; }

        public string? Departamento { get; set; }

    }
}
