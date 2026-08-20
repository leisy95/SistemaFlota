using SistemaFlota.DTOs.Costos.OrdenCompra;

public class OrdenCompraDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ProveedorId { get; set; }
    public string Proveedor { get; set; } = string.Empty;
    public DateTime FechaOrden { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public string FormaPago { get; set; } = string.Empty;
    public string LugarEntrega { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public decimal TotalKg { get; set; }
    public decimal TotalBultos { get; set; }
    public decimal KgRecibidos { get; set; }
    public decimal BultosRecibidos { get; set; }
    public decimal KgPendientes { get; set; }
    public decimal BultosPendientes { get; set; }
    public decimal Subtotal { get; set; }
    public string TipoImpuesto { get; set; } = "IVA";
    public decimal PorcentajeImpuesto { get; set; }
    public decimal ValorImpuesto { get; set; }
    public decimal TotalPagar { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? RecepcionId { get; set; }
    public string? Observaciones { get; set; }
    // Auditoría
    public string? UsuarioCreacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? UsuarioActualizacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public List<OrdenCompraDetalleDto> Detalles { get; set; } = [];
}