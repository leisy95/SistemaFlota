using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;

namespace SistemaFlota.Services.InventarioBodega.DashboardBodega
{
    public interface IMovimientosInventarioService
    {
        Task<IEnumerable<MovimientoInventarioResponseDto>>
            ObtenerMovimientosAsync();

        Task<MovimientoInventarioResponseDto?>
            ObtenerMovimientoPorIdAsync(int id);

        Task<MovimientoInventarioAuditoriaCompletaDto>
            ObtenerAuditoriaAsync(MovimientoInventarioFiltroDto filtros);
        Task<MovimientoInventarioFiltrosOpcionesDto> ObtenerOpcionesFiltrosAsync();

        Task<MovimientoInventarioResponseDto>
            RegistrarMovimientoAsync(MovimientoInventarioRequestDto request);
    }
}