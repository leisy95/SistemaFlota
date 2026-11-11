using SistemaFlota.DTOs.InventarioBodega.Inventario;

namespace SistemaFlota.Services.InventarioBodega.EntradasBodega
{
    public interface IEntradaInventarioService
    {
        Task<IngresarPesoResponseDto> IngresarPesoAsync(
            IngresarPesoDto request,
            string usuario);
    }
}
