using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaFlota
{
    public class TrazabilidadFactura
    {
        [Key]
        public int Id { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // ========================
        // VINCULACIÓN OPCIONAL CON AUTORIZACIÓN
        // ========================
        public int? AutorizacionId { get; set; }
        [ForeignKey("AutorizacionId")]
        public Autorizacion? Autorizacion { get; set; }

        // ========================
        // DATOS PRINCIPALES (editables)
        // ========================
        public string FacturaRemision { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Conductor { get; set; } = string.Empty;
        public string? Transportadora { get; set; }
        public string? Guia { get; set; }
        public string? Vehiculo { get; set; }

        // ========================
        // DATOS DE FLETE
        // ========================
        public decimal? PesoKilos { get; set; }
        public decimal? ValorFlete { get; set; }

        // ========================
        // VALIDACIONES
        // ========================
        // Ajuste recibido Sí/No
        public bool AjusteRecibido { get; set; } = false;

        // Factura entregada a empresa firmada Sí/No
        public bool FacturaEntregada { get; set; } = false;
        public DateTime? FechaEntrega { get; set; }

        // ========================
        // NOVEDAD
        // ========================
        public string? Novedad { get; set; }

        // ========================
        // ESTADO GENERAL
        // Pendiente | EnTransito | Entregado | Novedad
        // ========================
        public string Estado { get; set; } = "Pendiente";
    }

    public class NotaTrazabilidad
    {
        [Key]
        public int Id { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int TrazabilidadId { get; set; }
        [ForeignKey("TrazabilidadId")]
        public TrazabilidadFactura? Trazabilidad { get; set; }

        public string NumeroNota { get; set; } = string.Empty;
        public string? Cliente { get; set; }
        public string Conductor { get; set; } = string.Empty;
        public bool FacturaEntregada { get; set; } = false;
        public string? Observacion { get; set; }
    }
}