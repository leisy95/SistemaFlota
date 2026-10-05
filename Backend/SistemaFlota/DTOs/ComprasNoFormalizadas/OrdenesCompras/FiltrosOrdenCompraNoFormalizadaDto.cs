namespace SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesCompras
{
    public class FiltrosOrdenCompraNoFormalizadaDto
    {
        public List<string> Estados { get; set; } = new();
        public List<ProveedorOrdenCompraNoFormalizadaFiltroDto> Proveedores { get; set; } = new();
        public List<string> FormasPago { get; set; } = new();
    }
}
