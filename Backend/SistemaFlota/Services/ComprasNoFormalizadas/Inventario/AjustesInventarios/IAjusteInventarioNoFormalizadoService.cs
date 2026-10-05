using SistemaFlota.DTOs.ComprasNoFormalizadas.Inventario;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario.AjustesInventarios
{
    public interface IAjusteInventarioNoFormalizadoService
    {
        Task<AjusteInventarioNoFormalizadoDto> CrearAsync(
            CrearAjusteInventarioNoFormalizadoDto dto);

        Task<InventarioAjusteNoFormalizadoDto> ObtenerInventarioAsync(
            int inventarioId);

        Task<List<AjusteInventarioNoFormalizadoDto>> ObtenerHistorialAsync(
            int inventarioId);
    }
}
