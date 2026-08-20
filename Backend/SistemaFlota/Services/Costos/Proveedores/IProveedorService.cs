using SistemaFlota.DTOs.Prov_Materiales.Proveedores;

namespace SistemaFlota.Services.Costos.Proveedores
{
    public interface IProveedorService
    {
        Task<ProveedorPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            int page,
            int pageSize
        );

        Task<ProveedorDto?> ObtenerPorIdAsync(int id);

        Task<ProveedorDto> CrearAsync(CrearProveedorDto dto);

        Task<bool> ActualizarAsync(int id, ActualizarProveedorDto dto);

        Task<bool> EliminarAsync(int id);
    }
}
