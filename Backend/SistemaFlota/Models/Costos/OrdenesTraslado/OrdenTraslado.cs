namespace SistemaFlota.Models.Costos.OrdenesTraslado
{
    public class OrdenTraslado
    {
        public int Id { get; set; }

        public string NumeroOrden { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Destino { get; set; } = "Extrusión";

        public int UsuarioId { get; set; }

        public decimal TotalKg { get; set; }

        public decimal TotalBultos { get; set; }

        // Pendiente -> Verificando -> Confirmada
        public string Estado { get; set; } = "Pendiente";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime? FechaVerificacion { get; set; }
        public int? UsuarioVerificacionId { get; set; }

        public DateTime? FechaConfirmacion { get; set; }
        public int? UsuarioConfirmacionId { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;

        public virtual Usuario? UsuarioVerificacion { get; set; }

        public virtual Usuario? UsuarioConfirmacion { get; set; }

        public virtual ICollection<OrdenTrasladoDetalle> Detalles { get; set; }
            = new List<OrdenTrasladoDetalle>();
    }
}