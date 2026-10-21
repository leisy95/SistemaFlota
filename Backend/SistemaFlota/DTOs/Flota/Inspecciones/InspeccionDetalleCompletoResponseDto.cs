namespace SistemaFlota.DTOs.Flota.Inspecciones
{
    public class InspeccionDetalleCompletoResponseDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public int Kilometraje { get; set; }

        public string? FotoOdometro { get; set; }
        public string? FirmaCondutor { get; set; }

        public string EstadoGeneral { get; set; } = string.Empty;
        public string EmojiEstado { get; set; } = string.Empty;
        public double PorcentajeNoConf { get; set; }

        public int TotalItems { get; set; }
        public int TotalNoConformes { get; set; }

        public ConductorInspeccionDto Conductor { get; set; } = new();
        public VehiculoInspeccionDto Vehiculo { get; set; } = new();

        public List<InspeccionDetalleResponseDto> Detalles { get; set; } = new();
    }

    public class ConductorInspeccionDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class VehiculoInspeccionDto
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
    }
}