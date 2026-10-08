using SistemaFlota.DTOs.HistorialVersionOdo;

namespace SistemaFlota.Services.HistorialVersionOdo
{
    public interface IMovimientoProductoService
    {
        Task<PaginacionDto<MovimientoProductoDto>> ObtenerAsync(
            int pagina = 1,
            int porPagina = 50,
            string? buscar = null,
            string? producto = null,
            string? proveedor = null,
            string? estado = null,
            string? unidadMedida = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        Task<EstadisticasMovimientoProductoDto> ObtenerEstadisticasAsync(
            string? buscar = null,
            string? producto = null,
            string? proveedor = null,
            string? estado = null,
            string? unidadMedida = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        Task<MovimientoProductoDto?> ObtenerPorIdAsync(int id);
    }
}
