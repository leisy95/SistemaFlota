namespace SistemaFlota.DTOs.Calidad.Formatos
{
    public class LiberarFormatoDto
    {
        public bool? PuedeLiberarse { get; set; }

        public string? ExplicacionNoLiberado { get; set; }

        public string? FirmaDigital { get; set; }

        public string? CargoFirma { get; set; }

        public string? ProduccionKgHora { get; set; }

        public decimal? DesperdicioTotalKg { get; set; }
    }
}
