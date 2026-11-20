using SistemaFlota.DTOs.Produccion.LecturaImagen;

namespace SistemaFlota.Services.Produccion.LecturaImagen
{
    public interface ILectorReferenciaService
    {
        Task<LectorReferenciaDto> LeerReferenciaAsync(
            IFormFile imagen
        );
    }
}
