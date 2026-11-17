using SistemaFlota.DTOs.InventarioBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Models.InventariosBodega.EntradasBodega;
using SistemaFlota.Models.InventariosBodega.Inventario;

public interface IInventarioBodegaService
{
    Task<List<InventarioBodega>> ObtenerAsync();

    Task<List<string>> ObtenerSugerenciasAsync(string? buscar);

    Task<InventarioBodegaPaginadoResponseDto> ObtenerPaginadoAsync(
        string? buscar,
        string? material,
        string? color,
        int pagina,
        int pageSize);

    Task<InventarioBodega?> ObtenerPorReferenciaAsync(
        string referencia);

    Task<RegistrarItemResponseDto> RegistrarItemAsync(
        RegistrarItemDto request);

    Task<List<InventarioBodegaItem>> ObtenerItemsPorInventarioAsync(
        int idInventarioBodega);

    Task<bool> EditarInventarioAsync(
        int id,
        ActualizarInventarioBodegaDto request);

    Task<RecalcularInventarioBodegaResponseDto>
        RecalcularInventarioAsync();

    Task<bool> EliminarInventarioAsync(int id);
}