using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario
{
    public interface IInventarioNoFormalizadoService
    {
        Task ProcesarRecepcionAsync(int recepcionId);
        Task<InventarioNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            int page,
            int pageSize,
            bool puedeVerDatosNumericos);
        Task<List<ProveedorFiltroNoFormalizadoDto>> ObtenerProveedoresInventarioAsync();
        Task<List<string>> ObtenerCategoriasInventarioAsync();
        Task<byte[]> ExportarExcelAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            bool puedeVerDatosNumericos);
    }
}