namespace SistemaFlota.DTOs.Calidad.FormatoFGC008
{
    public class FormatoFGC008RespuestaDto
    {
        public int Id { get; set; }

        public string? OrdenProduccion { get; set; }

        public decimal AcumuladoOP { get; set; }

        public decimal PorHacer { get; set; }

        public decimal DiferenciaPct { get; set; }

        public string? RevisadoPorNombre { get; set; }
    }
}
