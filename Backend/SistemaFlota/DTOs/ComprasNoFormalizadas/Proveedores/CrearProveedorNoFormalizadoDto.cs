using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores
{
    public class CrearProveedorNoFormalizadoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Documento { get; set; }

        [StringLength(100)]
        public string? Contacto { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        public string? CorreoElectronico { get; set; }

        public string? Direccion { get; set; }

        public string? Ciudad { get; set; }

        public string? Departamento { get; set; }
    }
}
