namespace SistemaFlota.DTOs.InventarioBodega.DashboardBodega
{
    public class MovimientoInventarioFiltroDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public string? Referencia { get; set; }
        public string? Usuario { get; set; }
        public string? Cliente { get; set; }
        public string? Tipo { get; set; }
        public string? ReferenciaImpresa { get; set; }

        public int Pagina { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
