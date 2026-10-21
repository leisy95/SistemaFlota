namespace SistemaFlota.Services.Flota.Inspecciones
{
    public interface IInspeccionPdfService
    {
        Task<byte[]> GenerarPdfAsync(int id);
    }
}
