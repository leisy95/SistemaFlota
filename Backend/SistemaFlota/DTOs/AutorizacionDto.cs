namespace SistemaFlota.DTOs
{
    public class CrearAutorizacionDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string DestinoCompleto { get; set; } = string.Empty;
        public int CantidadClientes { get; set; }
        public decimal PesoKilos { get; set; }
        public string TipoVuelta { get; set; } = string.Empty;
        public string DescripcionCarga { get; set; } = string.Empty;
        public string? NumeroGuia { get; set; }
        public string? FacturasClientes { get; set; }
    }

    public class FirmaDto
    {
        public string Firma { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }

    public class LlegadaConductorDto
    {
        public int? KilometrajeFinal { get; set; }
        public string? NovedadesViaje { get; set; }
        public string EstadoVehiculo { get; set; } = "Bueno";
    }

    public class FacturaClienteDto
    {
        public string? FacturaRemision { get; set; }
        public string? Cliente { get; set; }
        public decimal? PesoKilos { get; set; }
    }

    public class SalidaRapidaDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string? TipoVuelta { get; set; }
        public string? DestinoCompleto { get; set; }
    }

    public class LlegadaRapidaDto
    {
        public int ConductorId { get; set; }
        public int VehiculoId { get; set; }
        public string? TipoVuelta { get; set; }
        public string? DestinoCompleto { get; set; }
        public int? KilometrajeFinal { get; set; }
        public string? NovedadesViaje { get; set; }
        public string? EstadoVehiculo { get; set; }
    }
}