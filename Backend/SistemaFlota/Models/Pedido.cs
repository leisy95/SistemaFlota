using System.ComponentModel.DataAnnotations;
namespace SistemaFlota
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        // ── VENDEDOR ──────────────────────────────────────────────────────────
        public string VendedorNombre { get; set; } = string.Empty;

        // ── DATOS DEL PEDIDO ──────────────────────────────────────────────────
        public string Cliente { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

        // ── CANTIDAD ──────────────────────────────────────────────────────────
        public decimal? CantidadKg { get; set; }
        public decimal? CantidadUnidades { get; set; }

        // ── PRIORIDAD ─────────────────────────────────────────────────────────
        // SOS | Urgente | Normal
        public string Prioridad { get; set; } = "Normal";

        // ── ESTADO ────────────────────────────────────────────────────────────
        // Pendiente | EnProceso | Despachado | Entregado
        public string Estado { get; set; } = "Pendiente";

        // ── OBSERVACIONES ─────────────────────────────────────────────────────
        public string? Observaciones { get; set; }

        // ── FECHAS ────────────────────────────────────────────────────────────
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaDespacho { get; set; }
        public DateTime? FechaEntrega { get; set; }

        // ── GESTIONADO POR ────────────────────────────────────────────────────
        public string? GestionadoPor { get; set; }

        public List<PedidoReferencia> Referencias { get; set; } = new();
    }
}