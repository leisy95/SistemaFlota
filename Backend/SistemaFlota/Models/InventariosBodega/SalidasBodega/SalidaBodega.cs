using SistemaFlota.Models.InventariosBodega.Clientes;

using System.ComponentModel.DataAnnotations;

namespace SistemaFlota.Models.InventariosBodega.SalidasBodega
{
    public class SalidaBodega
    {
        public int Id { get; set; }

        [Required]
        public string Usuario { get; set; } = string.Empty;

        // EN_PROCESO | CONFIRMADA | CANCELADA
        public string Estado { get; set; } = "EN_PROCESO";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaSalida { get; set; }

        public DateTime? FechaCancelacion { get; set; }

        public int? ClienteId { get; set; }

        public Cliente? Cliente { get; set; }

        public ICollection<SalidaBodegaDetalle> Detalles { get; set; }
            = new List<SalidaBodegaDetalle>();
    }
}