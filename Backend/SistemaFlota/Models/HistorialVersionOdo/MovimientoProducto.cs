namespace SistemaFlota.Models.HistorialVersionOdo
{
    public class MovimientoProducto
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Referencia { get; set; } = string.Empty;

        public string Producto { get; set; } = string.Empty;

        public string Proveedor { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public string UnidadMedida { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;
    }
}
