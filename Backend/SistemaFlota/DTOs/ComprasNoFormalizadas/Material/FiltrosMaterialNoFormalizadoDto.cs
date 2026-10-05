namespace SistemaFlota.DTOs.ComprasNoFormalizadas.Material
{
    public class FiltrosMaterialNoFormalizadoDto
    {
        public List<ProveedorNoFormalizadoFiltroDto> Proveedores { get; set; }
            = new();

        public List<string> Colores { get; set; }
            = new();
    }

}
