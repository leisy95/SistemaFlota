namespace SistemaFlota.DTOs.ControlEnvios.Trazabilidad
{
    public class TrazabilidadPaginadaDto
    {
        public List<TrazabilidadFactura> Data { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int PorPagina { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class TrazabilidadResumenDto
    {
        public int Total { get; set; }
        public int Entregadas { get; set; }
        public int PendientesEntrega { get; set; }
        public decimal TotalFlete { get; set; }
    }

    public class AutorizacionDisponibleDto
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string TipoVuelta { get; set; } = string.Empty;
        public string DestinoCompleto { get; set; } = string.Empty;
        public string? NumeroGuia { get; set; }
        public string? FacturasClientes { get; set; }
        public string Conductor { get; set; } = "-";
        public string Vehiculo { get; set; } = "-";
    }
}
