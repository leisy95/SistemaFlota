using SistemaFlota.DTOs.ComprasNoFormalizadas.Material;
using SistemaFlota.DTOs.ComprasNoFormalizadas.Proveedores;
using SistemaFlota.Models.Categorias;
using SistemaFlota.Models.Colores;

namespace SistemaFlota.Services.ComprasNoFormalizadas.Materiales
{
    public interface IMaterialNoFormalizadoService
    {
        Task<MaterialNoFormalizadoPaginadoDto> ObtenerAsync(
            string? search,
            string? estado,
            string? orden,
            string? proveedor,
            string? color,
            int page,
            int pageSize
        );

        Task<MaterialNoFormalizadoDto?> ObtenerPorIdAsync(int id);

        Task<MaterialNoFormalizadoDto> CrearAsync(
            CrearMaterialNoFormalizadoDto dto
        );

        Task<bool> ActualizarAsync(
            int id,
            ActualizarMaterialNoFormalizadoDto dto
        );

        Task<bool> EliminarAsync(int id);

        Task<FiltrosMaterialNoFormalizadoDto> ObtenerFiltrosAsync();

        Task<List<Color>> ObtenerColoresAsync();

        Task<List<Categoria>> ObtenerCategoriasAsync();
    }
}