namespace SistemaFlota.Services.ControlEnvios.CostosFlete.CostosFletePdf
{
    public interface ICostosFletesPdfService
    {
        Task<byte[]> GenerarPdfAsync(
            string? desde,
            string? hasta,
            string? conductor,
            string? estado,
            string? ciudad,
            string? placa);
    }
}
