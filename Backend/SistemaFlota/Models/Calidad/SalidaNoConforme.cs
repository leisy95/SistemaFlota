using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Calidad
{
    public class SalidaNoConforme
    {
        [Key] public int Id { get; set; }

        // ── PASO 1: Reportar ──
        public DateTime FechaReporte { get; set; } = DateTime.Now;
        [MaxLength(50)] public string HoraReporte { get; set; } = string.Empty;
        [Required][MaxLength(50)] public string OrdenProduccion { get; set; } = string.Empty;
        [MaxLength(200)] public string? Referencia { get; set; }
        public decimal? CantidadKg { get; set; }
        [MaxLength(200)] public string? Cliente { get; set; }
        [MaxLength(100)] public string? Linea { get; set; }
        [MaxLength(100)] public string? Material { get; set; }
        [MaxLength(100)] public string? Proceso { get; set; }
        public string? DescripcionSalida { get; set; }
        [MaxLength(100)] public string? TipoDefecto { get; set; }
        [MaxLength(100)] public string? Impacto { get; set; }
        [MaxLength(300)] public string? EvidenciaPdf { get; set; }
        public string? CausaRaiz { get; set; }
        public decimal? CantidadReportadaKg { get; set; }

        [MaxLength(20)] public string? UnidadCantidadReportada { get; set; } // Unidades | Bultos | Kg
        [MaxLength(200)] public string? UsuarioReporta { get; set; }
        public string? FirmaReporta { get; set; }
        [MaxLength(200)] public string? NombreReporta { get; set; }   


        // ── PASO 2: Tratamiento ──
        [MaxLength(100)] public string? TratamientoAdoptado { get; set; }
        public string? DescripcionTratamiento { get; set; }
        public DateTime? FechaTratamiento { get; set; }
        [MaxLength(200)] public string? UsuarioTratamiento { get; set; }
        public string? FirmaTratamiento { get; set; }
        [MaxLength(300)] public string? EvidenciaPdfTratamiento { get; set; }

        // ── PASO 3: Verificación / Cierre ──
        public string? VerificacionCumplimiento { get; set; }
        public DateTime? FechaVerificacion { get; set; }
      
        [MaxLength(300)] public string? EvidenciaPdfVerificacion { get; set; }
        public bool? RequiereInformacionCliente { get; set; }
        public string? MotivoInformacionCliente { get; set; }
        public bool? AceptacionBajoConcesion { get; set; }
        public string? DetalleAceptacionConcesion { get; set; }
        public string? FirmaVerificacion { get; set; }
        [MaxLength(200)] public string? RevisadoPor { get; set; }   
        


        // ── Control de estado ──
        [MaxLength(30)] public string Estado { get; set; } = "Reportado"; 
    }
}