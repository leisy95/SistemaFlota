
using SistemaFlota.DTOs.Flota.Inspecciones;

namespace SistemaFlota.Services.Flota.Inspecciones
{
    public interface IInspeccionService
    {

        Task<object> ObtenerInspeccionesAsync(
            int pagina = 1,
            int porPagina = 20,
            string? buscar = null,
            int? conductorId = null,
            int? vehiculoId = null,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null);

        Task<InspeccionDetalleCompletoResponseDto?> ObtenerDetalleAsync(
            int id);

        Task<InspeccionResponseDto> CrearAsync(
            CrearInspeccionDto dto,
            string usuario,
            string rol);
    }
}
