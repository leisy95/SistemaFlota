namespace SistemaFlota.DTOs
{
    public class CrearPedidoDto
    {
        public string VendedorNombre { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Prioridad { get; set; } = "Normal";
        public string? Observaciones { get; set; }
        public List<CrearReferenciaDto> Referencias { get; set; } = new();
    }

    public class CrearReferenciaDto
    {
        public string Referencia { get; set; } = string.Empty;
        public decimal? CantidadKg { get; set; }
        public decimal? CantidadUnidades { get; set; }
    }

    public class CambiarEstadoPedidoDto
    {
        public string Estado { get; set; } = string.Empty;
        public string? GestionadoPor { get; set; }
    }
}