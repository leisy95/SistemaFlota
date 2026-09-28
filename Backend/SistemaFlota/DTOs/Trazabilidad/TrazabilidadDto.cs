namespace SistemaFlota.DTOs.Trazabilidad
{
    public class CrearTrazabilidadDto
    {
        public int? AutorizacionId { get; set; }
        public string FacturaRemision { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Conductor { get; set; } = string.Empty;
        public string? Transportadora { get; set; }
        public string? Guia { get; set; }
        public string? Vehiculo { get; set; }
        public decimal? PesoKilos { get; set; }
        public decimal? ValorFlete { get; set; }
        public bool AjusteRecibido { get; set; }
        public bool FacturaEntregada { get; set; }
        public string? Novedad { get; set; }
        public string? Estado { get; set; }
    }

    public class CrearNotaDto
    {
        public string NumeroNota { get; set; } = string.Empty;
        public string? Cliente { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public bool FacturaEntregada { get; set; }
        public string? Observacion { get; set; }
    }
}