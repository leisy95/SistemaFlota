namespace SistemaFlota.DTOs.Costos.Materiales
{
    public class FiltrosMaterialDto
    {
        public List<ProveedorFiltroDto> Proveedores { get; set; } = new();
        public List<string> Colores { get; set; } = new();
    }

    public class ProveedorFiltroDto
    {
        public int IdProveedor { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
