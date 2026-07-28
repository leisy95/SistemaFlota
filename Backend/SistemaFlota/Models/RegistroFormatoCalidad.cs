using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SistemaFlota.Models
{
    public class RegistroFormatoCalidad
    {
        [Key] public int Id { get; set; }
        [Required] public int TipoFormatoId { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(200)] public string? Operarios { get; set; }
        [MaxLength(50)] public string? Hora { get; set; }
        [Required][MaxLength(50)] public string OrdenProduccion { get; set; } = string.Empty;
        [MaxLength(200)] public string? Cliente { get; set; }
        [MaxLength(200)] public string? Referencia { get; set; }
        [MaxLength(100)] public string? Maquina { get; set; }

        // Solo aplica si TieneVariablesCriticas = true (ej: Extrusión)
        public string? VariablesCriticasJson { get; set; }

        // Lista de resultados por característica: [{caracteristicaId, cumplePorcentaje, noCumplePorcentaje, na, observacion}]
        public string ResultadosJson { get; set; } = "[]";
        public string Estado { get; set; } = "PendienteLiberacion"; 
        public bool? PuedeLiberarse { get; set; }

        public string? ExplicacionNoLiberado { get; set; }

        public string? FirmaDigital { get; set; }
        [MaxLength(100)] public string? CargoFirma { get; set; }
        [MaxLength(200)] public string? RevisadoPor { get; set; }
        public DateTime? FechaRevision { get; set; }

        [ForeignKey("TipoFormatoId")]
        public TipoFormatoCalidad? TipoFormato { get; set; }
    }
}