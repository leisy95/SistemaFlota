namespace SistemaFlota.DTOs.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionFormularioNoFormalizadaDto
    {
        public int OrdenCompraNoFormalizadaId { get; set; }

        public string NumeroOrden { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public string? Recibe { get; set; }

        public string? Cargo { get; set; }

        public DateTime FechaOrden { get; set; }

        public List<RecepcionMercanciaDetalleFormularioNoFormalizadaDto> Items { get; set; }
            = new();
    }

    public class RecepcionMercanciaDetalleFormularioNoFormalizadaDto
    {
        public int OrdenCompraDetalleNoFormalizadaId { get; set; }

        public int MaterialNoFormalizadoId { get; set; }

        public string Material { get; set; } = string.Empty;

        // Pedido original
        public decimal Cantidad { get; set; }

        public decimal Bultos { get; set; }

        // Acumulado recibido
        public decimal CantidadRecibida { get; set; }

        public decimal BultosRecibidos { get; set; }

        // Pendiente por recibir
        public decimal CantidadPendiente { get; set; }

        public decimal BultosPendientes { get; set; }
    }
}
