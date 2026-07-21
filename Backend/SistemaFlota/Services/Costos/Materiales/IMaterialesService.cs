using SistemaFlota.DTOs.Costos.Materiales;
using SistemaFlota.DTOs.Prov_Materiales.Proveedores;

namespace SistemaFlota.Services.Costos.Materiales
{
    public interface IMaterialesService
    {
        Task<MaterialPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            string? proveedor,
            string? color,
            int page,
            int pageSize
        );

        Task<MaterialDto?> ObtenerPorIdAsync(int id);

        Task<MaterialDto> CrearAsync(CrearMaterialDto dto);

        Task<bool> ActualizarAsync(int id, ActualizarMaterialDto dto);

        Task<bool> EliminarAsync(int id);

        Task<FiltrosMaterialDto> ObtenerFiltrosAsync();
    }
}
