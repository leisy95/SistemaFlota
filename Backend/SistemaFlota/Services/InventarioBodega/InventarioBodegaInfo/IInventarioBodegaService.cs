using SistemaFlota.DTOs.InventarioBodega;
using SistemaFlota.DTOs.InventarioBodega.Inventario;
using SistemaFlota.Models.InventarioBodega;

public interface IInventarioBodegaService
{
    Task<List<InventarioBodega>> ObtenerAsync();

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

    Task<bool> EditarInventarioAsync(
        int id,
        ActualizarInventarioBodegaDto request);

    Task<RecalcularInventarioBodegaResponseDto>
        RecalcularInventarioAsync();

    Task<bool> EliminarInventarioAsync(int id);
}