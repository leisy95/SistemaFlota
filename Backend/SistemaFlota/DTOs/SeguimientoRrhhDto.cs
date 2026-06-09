namespace SistemaFlota.DTOs
{
    public class SeguimientoRrhhFotoDto
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string TipoFoto { get; set; } = string.Empty;
        public DateTime FechaSubida { get; set; }
    }

    public class SeguimientoRrhhDto
    {
        public int Id { get; set; }
        public string Area { get; set; } = string.Empty;
        public byte Mes { get; set; }
        public short Anio { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public string? Areas { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? PlanAccionSugerido { get; set; }
        public string? FactorRiesgo { get; set; }
        public string Prioridad { get; set; } = "Media";
        public string? Responsable { get; set; }
        public DateTime? FechaEjecucion { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Observaciones { get; set; }
        public int CreadoPor { get; set; }
        public string? NombreCreadoPor { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public List<SeguimientoRrhhFotoDto> Fotos { get; set; } = new();
    }

    public class CrearSeguimientoRrhhDto
    {
        public string Area { get; set; } = string.Empty;
        public byte Mes { get; set; }
        public short Anio { get; set; }
        public string Fuente { get; set; } = string.Empty;
        public string? Areas { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? PlanAccionSugerido { get; set; }
        public string? FactorRiesgo { get; set; }
        public string Prioridad { get; set; } = "Media";
        public string? Responsable { get; set; }
        public DateTime? FechaEjecucion { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Observaciones { get; set; }
    }

    public class ActualizarSeguimientoRrhhDto : CrearSeguimientoRrhhDto { }
}