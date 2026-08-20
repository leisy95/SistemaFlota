namespace SistemaFlota.Models.Costos.Inventario
{
    public class AjusteInventario
    {
        public int Id { get; set; }

        public string NumeroAjuste { get; set; } = string.Empty;

        public DateTime Fecha { get; set; } = DateTime.Now;

        public int InventarioId { get; set; }
        public Inventario Inventario { get; set; } = null!;
        public string Tipo { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal StockAnterior { get; set; }

        public decimal StockNuevo { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public decimal CostoPromedio { get; set; }

        public string? Observaciones { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
