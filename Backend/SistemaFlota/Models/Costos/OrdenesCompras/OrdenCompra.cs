using SistemaFlota.Models.Costos.RecepcionMercancias;
using SistemaFlota.Models.Proveedores;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota.Models.Costos.OrdenesCompras
{
    public class OrdenCompra
    {
        [Key]
        public int Id { get; set; }

        public string Numero { get; set; } = string.Empty;

        public int ProveedorId { get; set; }

        [ForeignKey(nameof(ProveedorId))]
        public virtual Proveedor? Proveedor { get; set; }

        public DateTime FechaOrden { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public string FormaPago { get; set; } = string.Empty;

        public string LugarEntrega { get; set; } = string.Empty;

        public int TotalItems { get; set; }

        public decimal TotalKg { get; set; }

        public decimal TotalBultos { get; set; }

        public decimal Subtotal { get; set; }

        public string TipoImpuesto { get; set; } = "IVA";

        public decimal PorcentajeImpuesto { get; set; } = 19;

        public decimal ValorImpuesto { get; set; }

        public decimal TotalPagar { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public string? Observaciones { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public int UsuarioCreacionId { get; set; }

        [ForeignKey(nameof(UsuarioCreacionId))]
        public virtual Usuario? UsuarioCreacion { get; set; }

        public int? UsuarioActualizacionId { get; set; }
        public DateTime? FechaActualizacion { get; set; }

        [ForeignKey(nameof(UsuarioActualizacionId))]
        public virtual Usuario? UsuarioActualizacion { get; set; }

        // Campos para Trazabilidad de envio
        public bool CorreoEnviado { get; set; } = false;
        public DateTime? FechaEnvioCorreo { get; set; }
        public int? UsuarioEnvioCorreoId { get; set; }

        public virtual ICollection<OrdenCompraDetalle> Detalles { get; set; }
            = new List<OrdenCompraDetalle>();

        public virtual ICollection<RecepcionMercancia> RecepcionesMercancia { get; set; }
            = new List<RecepcionMercancia>();

      
    }
}