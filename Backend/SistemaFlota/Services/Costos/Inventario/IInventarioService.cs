using SistemaFlota.DTOs.Costos.Inventario;

namespace SistemaFlota.Services.Costos.Inventario
{
    public interface IInventarioService
    {
        Task ProcesarRecepcionAsync(int recepcionId);

        Task<InventarioPaginadoDto> ObtenerAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color,
            int page,
            int pageSize);

        Task<List<ProveedorFiltroDto>> ObtenerProveedoresInventarioAsync();
        Task<List<string>> ObtenerCategoriasInventarioAsync();
        Task<byte[]> ExportarExcelAsync(
            string? search,
            int? proveedorId,
            string? categoria,
            string? color);
    }
}