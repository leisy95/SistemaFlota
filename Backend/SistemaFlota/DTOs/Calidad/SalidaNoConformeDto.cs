namespace SistemaFlota.DTOs
{
    public class CrearSalidaNoConformeDto
    {
        public string OrdenProduccion { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public decimal? CantidadKg { get; set; }
        public string? Cliente { get; set; }
        public string? Linea { get; set; }
        public string? Material { get; set; }
        public string? Proceso { get; set; }
        public string? DescripcionSalida { get; set; }
        public string? TipoDefecto { get; set; }
        public string? Impacto { get; set; }
        public string? CausaRaiz { get; set; }
        public decimal? CantidadReportadaKg { get; set; }

        public string? UnidadCantidadReportada { get; set; }
        public string? FirmaReporta { get; set; }
        public string? NombreReporta { get; set; }
    }

    public class TratamientoSncDto
    {
        public string? TratamientoAdoptado { get; set; }
        public string? DescripcionTratamiento { get; set; }
        public DateTime? FechaTratamiento { get; set; }
        public string? FirmaTratamiento { get; set; }
    }

    public class VerificacionSncDto
    {
        public string? VerificacionCumplimiento { get; set; }
        public bool? RequiereInformacionCliente { get; set; }
        public string? MotivoInformacionCliente { get; set; }
        public bool? AceptacionBajoConcesion { get; set; }
        public string? DetalleAceptacionConcesion { get; set; }
        public string? FirmaVerificacion { get; set; }
        public string? RevisadoPor { get; set; }
    }
}
