namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public interface ICorteInventarioNoFormalizadoPdfService
    {
        Task<byte[]> GenerarPdfAsync();
    }
}
