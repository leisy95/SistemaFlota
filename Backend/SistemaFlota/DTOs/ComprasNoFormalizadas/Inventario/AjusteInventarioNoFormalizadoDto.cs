namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario
{
    public class AjusteInventarioNoFormalizadoDto
    {
        public int Id { get; set; }

        public string NumeroAjuste { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public string Usuario { get; set; } = string.Empty;

        public string Material { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public decimal StockAnterior { get; set; }

        public decimal StockNuevo { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }
}
