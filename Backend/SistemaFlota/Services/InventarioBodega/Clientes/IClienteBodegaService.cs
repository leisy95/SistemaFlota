using SistemaFlota.DTOs.InventarioBodega.Clientes;

namespace SistemaFlota.Services.InventarioBodega.Clientes
{
    public interface IClienteBodegaService
    {
        Task<ClientesBodegaPaginadosDto> ListarClientesAsync(
            ClienteBodegaFiltroDto filtros);

        Task<ClienteResponseDto?> ObtenerClienteAsync(int id);

        Task<RespuestaClienteDto> CrearClienteAsync(ClienteRequestDto dto);

        Task<RespuestaClienteDto> ActualizarClienteAsync(
            int id, EditarClienteRequestDto dto);

        Task<RespuestaClienteDto> EliminarClienteAsync(int id);
    }
}