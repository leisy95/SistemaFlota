using System.ComponentModel.DataAnnotations;
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

        // Verificaciones (las llena una persona)
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

        // Origen y unidades
        [MaxLength(20)] public string Origen { get; set; } = "Manual";   
        [MaxLength(10)] public string? UnidadOP { get; set; }            
        [MaxLength(10)] public string? UnidadReal { get; set; }          

        // Datos que llegan de Bodega (sincronización automática)
        [MaxLength(30)] public string? Seccion { get; set; }
        [MaxLength(300)] public string? OperariosBodega { get; set; }
        public DateTime? DiaEntrada { get; set; }
        public int Bultos { get; set; }
        [MaxLength(10)] public string? Troquel { get; set; }           
        [Column(TypeName = "decimal(18,2)")] public decimal UnidadesEntrada { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal KilosEntrada { get; set; }
        public int? UltimoNumEntrada { get; set; }
        public bool EnProceso { get; set; }
        public bool CambioDespuesDeRevisar { get; set; }
        public DateTime? UltimaSincronizacion { get; set; }
        public List<FormatoFGC008Foto> FotosEvidencia { get; set; } = new();
        [NotMapped] public decimal? AcumuladoOP { get; set; }
        [NotMapped] public decimal? PorHacer { get; set; }
        [NotMapped] public decimal? DiferenciaPct { get; set; }
        [NotMapped] public string? RevisadoPorNombre { get; set; }
    }
}