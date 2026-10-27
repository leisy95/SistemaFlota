namespace SistemaFlota.DTOs.Calidad.FormatoFGC008
{
    public class FormatoFGC008ResumenDto
    {
        public int Total { get; set; }

        public int ConDefectos { get; set; }

        public int PendientesDespacho { get; set; }

        public int PorcentajeCompletado { get; set; }
    }
}
