using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota
{
    [Table("FormatoFGC008")]
    public class FormatoFGC008
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string OrdenProduccion { get; set; } = string.Empty;
        public string? Cliente { get; set; }
        public string? Referencia { get; set; }
        public bool EtiquetasSI { get; set; }
        public bool EmbalajeSI { get; set; }
        public bool DefectosSI { get; set; }
        public decimal CantidadOP { get; set; }
        public decimal CantidadReal { get; set; }
        public bool ListoBodega { get; set; }
        public string? Despachado { get; set; }
        public string? AccionesTomadas { get; set; }
        public string? FotoEvidencia { get; set; }
        public string? RevisadoPor { get; set; }

        public string? FirmaDigital { get; set; }
        public DateTime? FechaRevision { get; set; }
    }
}