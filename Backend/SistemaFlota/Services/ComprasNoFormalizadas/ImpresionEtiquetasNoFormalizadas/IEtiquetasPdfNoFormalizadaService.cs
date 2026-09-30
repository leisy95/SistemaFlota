namespace SistemaFlota.Services.ComprasNoFormalizadas.ImpresionEtiquetasNoFormalizadas
{
    public interface IEtiquetasPdfNoFormalizadaService
    {
        Task<byte[]> GenerarAsync(int recepcionId);
    }
}
