namespace SistemaFlota.Services.RecursosHumanos.RecursosHumanosPdf
{
    public interface ISeguimientosRrhhPdfService
    {
        Task<byte[]> GenerarPdfAsync(
            string? area,
            string? estado,
            string? prioridad,
            int? mes,
            int? anio
        );
    }
}
