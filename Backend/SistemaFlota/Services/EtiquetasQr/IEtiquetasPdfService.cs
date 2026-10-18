namespace SistemaFlota.Services.ImpresionEtiquetas
{
    public interface IEtiquetasPdfService
    {
        Task<byte[]> GenerarAsync(int recepcionId);

        // Nueva impresión de todas las etiquetas 
        Task<byte[]> GenerarTodasAsync(int recepcionId);
    }
}