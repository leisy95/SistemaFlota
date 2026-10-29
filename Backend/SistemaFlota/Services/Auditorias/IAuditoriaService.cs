using SistemaFlota.DTOs.Auditorias;

namespace SistemaFlota.Services.Auditorias
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(
            string usuario,
            string rol,
            string accion,
            string modulo,
            string? detalle = null,
            int? registroId = null,
            string resultado = "Exitoso");

        Task<List<AuditoriaDto>> ObtenerTodosAsync(
            AuditoriaFiltroDto filtro);

        Task<AuditoriaPaginadaDto> ObtenerPaginadoAsync(
            AuditoriaFiltroDto filtro,
            int pagina = 1,
            int porPagina = 25);

        Task<object> ObtenerEstadisticasAsync();

        Task<int> LimpiarAsync(int dias = 90);
    }
}