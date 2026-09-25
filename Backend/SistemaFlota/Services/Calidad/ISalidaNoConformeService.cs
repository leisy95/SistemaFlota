using SistemaFlota.DTOs;
using SistemaFlota.Models.Calidad;

namespace SistemaFlota.Services.Calidad
{
    public interface ISalidaNoConformeService
    {
        Task<List<SalidaNoConforme>> ListarAsync(DateTime? desde, DateTime? hasta, string? referencia, string? material, string? ordenProduccion, string? tipoDefecto, string? proceso);
        Task<SalidaNoConforme?> ObtenerAsync(int id);
        Task<SalidaNoConforme> CrearAsync(CrearSalidaNoConformeDto dto, string usuario, IFormFile? evidenciaPdf);
        Task<SalidaNoConforme?> RegistrarTratamientoAsync(int id, TratamientoSncDto dto, string usuario);
        Task<SalidaNoConforme?> CerrarAsync(int id, VerificacionSncDto dto, IFormFile? evidenciaPdf);
        Task<bool> EliminarAsync(int id, bool esAdmin);
        decimal SumarKgDelMes(List<SalidaNoConforme> registros, int mes, int anio);
        Task<List<SalidaNoConformeEvidencia>> SubirEvidenciasAsync(int salidaNoConformeId, string paso, List<IFormFile> fotos);
        Task<List<SalidaNoConformeEvidencia>> ObtenerEvidenciasAsync(int salidaNoConformeId);
    }
}