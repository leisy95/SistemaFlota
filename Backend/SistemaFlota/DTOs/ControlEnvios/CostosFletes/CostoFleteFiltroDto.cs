namespace SistemaFlota.DTOs.ControlEnvios.CostosFletes
{
    public class CostoFleteFiltroDto
    {
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }

        public string? Conductor { get; set; }
        public string? Estado { get; set; }
        public string? Ciudad { get; set; }
        public string? Placa { get; set; }
    }
}
