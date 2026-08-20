namespace SistemaFlota.Services.ImpresionEtiquetas
{
    public interface IEtiquetasPdfService
    {
        Task<byte[]> GenerarAsync(int recepcionId);
    }
}