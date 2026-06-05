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
        public string Destino { get; set; } = string.Empty;

        // ── PRIORIDAD ─────────────────────────────────────────────────────────
        public string Prioridad { get; set; } = "Normal";

        // ── ESTADO ────────────────────────────────────────────────────────────
        public string Estado { get; set; } = "Pendiente";

        // ── OBSERVACIONES ─────────────────────────────────────────────────────
        public string? Observaciones { get; set; }

        // ── FECHAS ────────────────────────────────────────────────────────────
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public DateTime? FechaDespacho { get; set; }
        public DateTime? FechaEntrega { get; set; }

        // ── GESTIONADO POR ────────────────────────────────────────────────────
        public string? GestionadoPor { get; set; }

        // ── REFERENCIAS ───────────────────────────────────────────────────────
        public List<PedidoReferencia> Referencias { get; set; } = new();
    }
}