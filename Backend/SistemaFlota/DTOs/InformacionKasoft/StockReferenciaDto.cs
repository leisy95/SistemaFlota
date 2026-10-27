namespace SistemaFlota.DTOs.InformacionKasoft
{
    public class StockReferenciaDto
    {
        public string Refer { get; set; } = string.Empty;

        public string Descrip { get; set; } = string.Empty;

        public DateTime? Corteant { get; set; }

        public double? Stockant { get; set; }

        public double? Entradas { get; set; }

        public double? Salidas { get; set; }

        public DateTime Corteact { get; set; }

        public double? Stockact { get; set; }

        public double? Pesoact { get; set; }

        public DateTime Fechahora { get; set; }
    }
}
