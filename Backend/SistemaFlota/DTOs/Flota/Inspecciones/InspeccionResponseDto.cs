namespace SistemaFlota.DTOs.Flota.Inspecciones
{
    public class InspeccionResponseDto
    {
        public int Id { get; set; }

        public string EstadoGeneral { get; set; } = string.Empty;

        public string EmojiEstado { get; set; } = string.Empty;

        public double PorcentajeNoConf { get; set; }

        public int TotalItems { get; set; }

        public int TotalNoConformes { get; set; }
    }
}
