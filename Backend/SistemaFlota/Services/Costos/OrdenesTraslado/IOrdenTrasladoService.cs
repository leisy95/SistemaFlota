using SistemaFlota.DTOs.Costos.OrdenesTraslado;

namespace SistemaFlota.Services.Costos.OrdenesTraslado
{
    public interface IOrdenTrasladoService
    {
        Task<OrdenTrasladoDto> CrearAsync(CrearOrdenTrasladoDto dto);

        Task<OrdenTrasladoDto?> ObtenerPorIdAsync(int id);

        Task<OrdenTrasladoPaginadoDto> ObtenerTodosAsync(
            string? search,
            string? estado,
            string? destino,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int pagina = 1,
            int tamanoPagina = 10);

        // Verificar materiales y cambiar la orden a "Verificando"
        Task<OrdenTrasladoDto> VerificarAsync(VerificarOrdenTrasladoDto dto);

        // Confirmar definitivamente la orden
        Task<OrdenTrasladoDto> ConfirmarAsync(int id);
    }
}