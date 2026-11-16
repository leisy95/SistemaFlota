using SistemaFlota.DTOs.InventarioBodega.DashboardBodega;

namespace SistemaFlota.Services.InventarioBodega.DashboardBodega
{
    public interface IMovimientosInventarioService
    {
        // Obtener todos los movimientos de la empresa
        Task<IEnumerable<MovimientoInventarioResponseDto>> ObtenerMovimientosAsync();

        // Obtener movimiento por ID
        Task<MovimientoInventarioResponseDto?> ObtenerMovimientoPorIdAsync(int id);

        // Obtener auditoría con filtros de fechas
        Task<MovimientoInventarioAuditoriaCompletaDto> ObtenerAuditoriaAsync(
            DateTime? fechaInicio,
            DateTime? fechaFin);

        // Registrar movimiento
        Task<MovimientoInventarioResponseDto> RegistrarMovimientoAsync(
            MovimientoInventarioRequestDto request);
    }
}
