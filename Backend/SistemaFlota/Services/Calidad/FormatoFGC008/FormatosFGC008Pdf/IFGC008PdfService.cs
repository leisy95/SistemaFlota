namespace SistemaFlota.Services.Calidad.FormatoFGC008.FormatosFGC008Pdf
{
    public interface IFGC008PdfService
    {
        Task<byte[]> GenerarPdfAsync(
            DateTime? desde,
            DateTime? hasta,
            string? op
        );
    }
}
