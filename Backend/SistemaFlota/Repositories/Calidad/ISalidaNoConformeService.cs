using SistemaFlota.Models.Calidad;

namespace SistemaFlota.Repositories.Calidad
{
    public interface ISalidaNoConformeRepository
    {
        Task<List<SalidaNoConforme>> ObtenerTodosAsync(DateTime? desde, DateTime? hasta, string? referencia, string? material, string? ordenProduccion, string? tipoDefecto, string? proceso);
        Task<SalidaNoConforme?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(SalidaNoConforme entidad);
        Task GuardarCambiosAsync();
        Task AgregarEvidenciasAsync(List<SalidaNoConformeEvidencia> evidencias);
        Task<List<SalidaNoConformeEvidencia>> ObtenerEvidenciasAsync(int salidaNoConformeId);
        void Eliminar(SalidaNoConforme entidad);
    }
}