namespace SistemaFlota.Services.EtiquetasQr.ComprasNoFormalizadas.EtiquetasNoFormalizadas
{
    public interface IEtiquetasPdfNoFormalizadaService
    {
        Task<byte[]> GenerarAsync(int recepcionId);
    }
}