using SistemaFlota.Models.Costos.OrdenesCompras;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Costos.RecepcionMercancias
{
    public class RecepcionMercancia
    {
        [Key]
        public int Id { get; set; }

        public string NumeroRecepcion { get; set; } = string.Empty;

        public int OrdenCompraId { get; set; }

        [ForeignKey(nameof(OrdenCompraId))]
        public virtual OrdenCompra? OrdenCompra { get; set; }

        public string Conductor { get; set; } = string.Empty;

        public string Transportadora { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = "Factura";

        public bool EmbalajeAdecuado { get; set; }

        public string Recibe { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public DateTime FechaRecepcion { get; set; } = DateTime.Now;

        // Para estado confirmacion
        public DateTime? FechaConfirmacion { get; set; }

        // NUEVO
        public int? UsuarioConfirmacionId { get; set; }

        [ForeignKey(nameof(UsuarioConfirmacionId))]
        public virtual Usuario? UsuarioConfirmacion { get; set; }
        public virtual ICollection<RecepcionMercanciaDetalle> Detalles { get; set; }
            = new List<RecepcionMercanciaDetalle>();
    }
}
