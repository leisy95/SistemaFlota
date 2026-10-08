namespace SistemaFlota.Models.HistorialVersionOdo
{
    public class PedidoCompra
    {
        public int Id { get; set; }

        public string Prioridad { get; set; } = string.Empty;

        public string ReferenciaOrden { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public string Comprador { get; set; } = string.Empty;

        public DateTime FechaLimiteOrden { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
