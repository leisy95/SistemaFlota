using SistemaFlota.DTOs.Costos.Inventario;

namespace SistemaFlota.Services.Costos.Inventario
{
    public interface IAjusteInventarioService
    {
        Task<AjusteInventarioDto> CrearAsync(CrearAjusteInventarioDto dto);

        Task<InventarioAjusteDto> ObtenerInventarioAsync(int inventarioId);

        Task<List<AjusteInventarioDto>> ObtenerHistorialAsync(int inventarioId);
    }
}