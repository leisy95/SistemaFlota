using SistemaFlota.Models.ComprasNoFormalizadas.OrdenesCompras;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.ComprasNoFormalizadas.RecepcionMercancias
{
    public class RecepcionMercanciaNoFormalizada
    {
        [Key]
        public int Id { get; set; }

        public string NumeroRecepcion { get; set; } = string.Empty;

        public int OrdenCompraNoFormalizadaId { get; set; }

        [ForeignKey(nameof(OrdenCompraNoFormalizadaId))]
        public virtual OrdenCompraNoFormalizada? OrdenCompraNoFormalizada { get; set; }

        public string Conductor { get; set; } = string.Empty;

        public string Transportadora { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = "Factura";

        public bool EmbalajeAdecuado { get; set; }

        public string Recibe { get; set; } = string.Empty;

        public string Cargo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }

        public DateTime FechaRecepcion { get; set; } = DateTime.Now;

        public DateTime? FechaConfirmacion { get; set; }

        public int? UsuarioConfirmacionId { get; set; }

        [ForeignKey(nameof(UsuarioConfirmacionId))]
        public virtual Usuario? UsuarioConfirmacion { get; set; }

        public virtual ICollection<RecepcionMercanciaDetalleNoFormalizada> Detalles { get; set; }
            = new List<RecepcionMercanciaDetalleNoFormalizada>();
    }
}
