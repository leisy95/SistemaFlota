
using SistemaFlota.DTOs.Costos.HistorialMovimientos;

namespace SistemaFlota.Services.Costos.HistorialMovimientos
{
    public interface IHistorialMovimientosService
    {
        Task<MovimientoHistorialPaginadoDto> ObtenerAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar,
            int pagina,
            int porPagina);

        Task<MovimientoHistorialResumenDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar);

        Task<MovimientoHistorialOpcionesDto> ObtenerOpcionesAsync();

        Task<MovimientoHistorialDto?> ObtenerPorIdAsync(int id);
    }
}
