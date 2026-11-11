namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class RegistrarItemDto
    {
        public string Referencia { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
        public string? TipoBolsa { get; set; }
        public string? TipoMaterial { get; set; }

        public string? Densidad { get; set; }

        public string? Color { get; set; }

        public decimal? FuelleLateral { get; set; }
        public decimal? FuelleFondo { get; set; }
        public decimal? FuelleSuperior { get; set; }

        public string? SegundoColor { get; set; }
        public string? ImpresoNo { get; set; }

        public decimal? Ancho { get; set; }
        public decimal? Alto { get; set; }
        public decimal? Calibre { get; set; }

        public string? Troquel { get; set; }
        public string? ReferenciaImpresa { get; set; }

        public decimal? PaqueteNumero { get; set; }
        public decimal? PaqueteCantidad { get; set; }
    }
}
