namespace SistemaFlota.DTOs
{
    public class RegistroFormatoCalidadDto
    {
        public int TipoFormatoId { get; set; }
        public string OrdenProduccion { get; set; } = string.Empty;
        public string? Cliente { get; set; }
        public string? Referencia { get; set; }
        public string? Operarios { get; set; }
        public string? Hora { get; set; }
        public string? Maquina { get; set; }
        public string? VariablesCriticasJson { get; set; }
        public string ResultadosJson { get; set; } = "[]";
        public bool? PuedeLiberarse { get; set; }
        public string? ExplicacionNoLiberado { get; set; }
        public string? FirmaDigital { get; set; }
        public string? CargoFirma { get; set; }
    }
}