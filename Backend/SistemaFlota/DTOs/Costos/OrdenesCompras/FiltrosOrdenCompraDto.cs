namespace SistemaFlota.DTOs.Costos.OrdenCompra
{
    public class FiltrosOrdenCompraDto
    {
        public List<string> Estados { get; set; } = [];

        public List<ProveedorFiltroDto> Proveedores { get; set; } = [];

        public List<string> FormasPago { get; set; } = [];
    }

    public class ProveedorFiltroDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}
