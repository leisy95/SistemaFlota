namespace SistemaFlota.DTOs.Costos.OrdenCompra
{
    public class FiltrosOrdenCompraDto
    {
        public List<string> Estados { get; set; } = [];
        public List<ProveedorOrdenCompraFiltroDto> Proveedores { get; set; } = [];
        public List<string> FormasPago { get; set; } = [];
    }

    public class ProveedorOrdenCompraFiltroDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}