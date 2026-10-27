namespace SistemaFlota.DTOs.Calidad.Formatos
{
    public class FotoFormatoCalidadDto
    {
        public int Id { get; set; }

        public string RondaId { get; set; } = string.Empty;

        public string Archivo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public string? SubidaPor { get; set; }
    }
}
