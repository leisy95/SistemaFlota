namespace SistemaFlota.Services.Calidad.Formatos.FormatosGenericos
{
    namespace SistemaFlota.Services.Calidad.Formatos.FormatosGenericos
    {
        public interface IFormatoCalidadPdfService
        {
            Task<byte[]> GenerarPdfAsync(
                string codigo,
                DateTime? desde,
                DateTime? hasta,
                string? op);
        }
    }
}
