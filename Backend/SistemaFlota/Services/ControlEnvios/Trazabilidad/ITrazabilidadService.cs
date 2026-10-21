
using SistemaFlota.DTOs.ControlEnvios.Trazabilidad;

namespace SistemaFlota.Services.ControlEnvios.Trazabilidad
{
    public interface ITrazabilidadService
    {
        Task<TrazabilidadPaginadaDto> ObtenerAsync(
            int pagina,
            int porPagina,
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta);

        Task<TrazabilidadResumenDto> ObtenerResumenAsync(
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta);

        Task<TrazabilidadFactura?> ObtenerPorIdAsync(int id);

        Task<TrazabilidadFactura> CrearAsync(
            CrearTrazabilidadDto dto,
            string usuario,
            string rol);

        Task<TrazabilidadFactura?> ActualizarAsync(
            int id,
            CrearTrazabilidadDto dto,
            string usuario,
            string rol);

        Task<bool> EliminarAsync(
            int id,
            string usuario,
            string rol);

        Task<List<NotaTrazabilidad>> ObtenerNotasAsync(int id);

        Task<NotaTrazabilidad?> AgregarNotaAsync(
            int id,
            CrearNotaDto dto,
            string usuario,
            string rol);

        Task<NotaTrazabilidad?> EditarNotaAsync(
            int notaId,
            CrearNotaDto dto);

        Task<bool> EliminarNotaAsync(int notaId);

        Task<List<AutorizacionDisponibleDto>>
            ObtenerAutorizacionesDisponiblesAsync();
    }
}
