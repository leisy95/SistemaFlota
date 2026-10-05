using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Proveedores
{
    public interface IProveedorNoFormalizadoService
    {
        Task<ProveedorNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            int page,
            int pageSize
        );

        Task<ProveedorNoFormalizadoDto?> ObtenerPorIdAsync(int id);

        Task<ProveedorNoFormalizadoDto> CrearAsync(
            CrearProveedorNoFormalizadoDto dto
        );

        Task<bool> ActualizarAsync(
            int id,
            ActualizarProveedorNoFormalizadoDto dto
        );
    }
}
