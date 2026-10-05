using SistemaFlota.DTOs.ComprasNoFormalizadas.OrdenesTraslado;

namespace SistemaFlota.Services.ComprasNoFormalizadas.OrdenesTraslado
{
    public interface IOrdenTrasladoNoFormalizadaService
    {
        Task<OrdenTrasladoNoFormalizadaDto> CrearAsync(CrearOrdenTrasladoNoFormalizadaDto dto);

        Task<OrdenTrasladoNoFormalizadaDto?> ObtenerPorIdAsync(int id);

        Task<OrdenTrasladoNoFormalizadaPaginadoDto> ObtenerTodosAsync(
            string? search,
            string? estado,
            string? destino,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1,
            int tamanoPagina = 10);

        Task<OrdenTrasladoNoFormalizadaDto> VerificarAsync(
            VerificarOrdenTrasladoNoFormalizadaDto dto);

        Task<OrdenTrasladoNoFormalizadaDto> ConfirmarAsync(int id);
    }
}
