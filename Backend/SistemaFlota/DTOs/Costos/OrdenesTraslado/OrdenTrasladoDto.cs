namespace SistemaFlota.DTOs.Costos.OrdenesTraslado
{
    public class OrdenTrasladoDto
    {
        public int Id { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Destino { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

        public int UsuarioId { get; set; }
        public string Usuario { get; set; } = string.Empty;

        public decimal TotalKg { get; set; }
        public decimal TotalBultos { get; set; }

        public DateTime? FechaVerificacion { get; set; }
        public int? UsuarioVerificacionId { get; set; }
        public string UsuarioVerificacion { get; set; } = string.Empty;

        public DateTime? FechaConfirmacion { get; set; }
        public int? UsuarioConfirmacionId { get; set; }
        public string UsuarioConfirmacion { get; set; } = string.Empty;

        public List<OrdenTrasladoDetalleDto> Materiales { get; set; } = new();
    }
}