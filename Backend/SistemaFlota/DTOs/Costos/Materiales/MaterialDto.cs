namespace SistemaFlota.DTOs.Costos.Materiales
{
    public class MaterialDto
    {
        public int IdMaterial { get; set; }

        public int IdProveedor { get; set; }

        public string Proveedor { get; set; } = string.Empty;

        public string NombreMaterial { get; set; } = string.Empty;

        public string? DescripcionCompra { get; set; }

        public string Densidad { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? TipoProduccion { get; set; }

        public string Unidad { get; set; } = string.Empty;

        public decimal PrecioBaseKg { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}
