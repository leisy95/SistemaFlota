using SistemaFlota.DTOs.InventarioBodega.HistorialMovimientos;

namespace SistemaFlota.Services.InventarioBodega.HistorialMovimientos
{
    public interface IHistorialInventarioBodegaService
    {
        Task<MovimientoInventarioBodegaPaginadoDto> ObtenerAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar,
            int pagina,
            int porPagina);

        Task<MovimientoInventarioBodegaResumenDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? modulo,
            string? accion,
            string? usuario,
            string? buscar);

        Task<MovimientoInventarioBodegaOpcionesDto> ObtenerOpcionesAsync();

        Task<MovimientoInventarioBodegaDto?> ObtenerPorIdAsync(int id);
    }
}
