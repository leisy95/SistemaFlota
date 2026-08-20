namespace SistemaFlota.DTOs.Costos.EtiquetasQr
{
    public class EtiquetaQrDto
    {
        public string NumeroRecepcion { get; set; } = "";

        public string Material { get; set; } = "";

        public string LoteProveedor { get; set; } = "";

        public decimal Peso { get; set; }

        public int NumeroBulto { get; set; }

        public int TotalBultos { get; set; }

        public string CodigoQr { get; set; } = "";
    }
}
