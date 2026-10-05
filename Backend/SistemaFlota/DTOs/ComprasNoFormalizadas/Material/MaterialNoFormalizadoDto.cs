namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Material
{
    public class MaterialNoFormalizadoDto
    {
        public int IdMaterialNoFormalizado { get; set; }

        public int IdProveedorNoFormalizado { get; set; }

        public string Proveedor { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string NombreMaterial { get; set; } = string.Empty;

        public string? DescripcionCompra { get; set; }

        public string Densidad { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? TipoProduccion { get; set; }

        public string Unidad { get; set; } = string.Empty;

        public decimal PrecioBaseKg { get; set; }

        public string? DocumentoPdf { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }
    }
}
