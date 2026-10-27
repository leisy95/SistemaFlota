namespace SistemaFlota.DTOs.InformacionKasoft
{
    public class EntradaDto
    {
        public int NumEntrada { get; set; }
        public DateTime? Fecha { get; set; }
        public int? NumeroOp { get; set; }
        public string? Seccion { get; set; }
        public string? Operario { get; set; }
        public int? TotalUnidades { get; set; }
        public decimal? TotalPeso { get; set; }
        public string? Observ { get; set; }
        public string? Troquel { get; set; }
        public string? Logoemp { get; set; }
        public int? NBulto { get; set; }
        public int? Unidades { get; set; }
        public decimal? Peso { get; set; }
        public DateTime? Fechahora { get; set; }
    }
}
