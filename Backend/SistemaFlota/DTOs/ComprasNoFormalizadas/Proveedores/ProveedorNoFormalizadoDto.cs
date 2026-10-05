namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores
{
    public class ProveedorNoFormalizadoDto
    {
        public int IdProveedorNoFormalizado { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Documento { get; set; }

        public string? Contacto { get; set; }

        public string? Telefono { get; set; }

        public string? CorreoElectronico { get; set; }

        public string? Direccion { get; set; }

        public string? Ciudad { get; set; }

        public string? Departamento { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}
