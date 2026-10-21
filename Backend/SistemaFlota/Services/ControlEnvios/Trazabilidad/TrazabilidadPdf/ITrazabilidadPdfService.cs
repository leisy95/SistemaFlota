namespace SistemaFlota.Services.ControlEnvios.Trazabilidad.TrazabilidadPdf
{
    public interface ITrazabilidadPdfService
    {
        Task<byte[]> GenerarPdfAsync(
            string? buscar,
            string? estado,
            string? entregada,
            string? tipo,
            DateTime? fechaDesde,
            DateTime? fechaHasta);
            
    }
}
