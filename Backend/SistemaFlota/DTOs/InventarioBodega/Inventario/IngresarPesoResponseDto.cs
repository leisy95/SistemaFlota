namespace SistemaFlota.DTOs.InventarioBodega.Inventario
{
    public class IngresarPesoResponseDto
    {
        public string Referencia { get; set; } = string.Empty;
        public string ReferenciaPeso { get; set; } = string.Empty;

        public decimal Peso { get; set; }
        public int Cantidad { get; set; }

        public string? Descripcion { get; set; }

        public decimal Ancho { get; set; }
        public decimal Alto { get; set; }
        public decimal Calibre { get; set; }

        public string? Color { get; set; }
        public string? Material { get; set; }

        public decimal FuelleLateral { get; set; }
        public decimal FuelleFondo { get; set; }
        public decimal FuelleSuperior { get; set; }

        public string? Troquel { get; set; }
        public string? ReferenciaImpresa { get; set; }

        public decimal PaqueteNumero { get; set; }
        public decimal PaqueteCantidad { get; set; }
    }
}