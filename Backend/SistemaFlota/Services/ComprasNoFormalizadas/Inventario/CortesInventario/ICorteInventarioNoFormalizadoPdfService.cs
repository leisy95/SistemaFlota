namespace SistemaFlota.Services.ComprasNoFormalizadas.Inventario.CortesInventario
{
    public interface ICorteInventarioNoFormalizadoPdfService { Task<byte[]> GenerarPdfAsync(string? material, string? proveedor); }
}
