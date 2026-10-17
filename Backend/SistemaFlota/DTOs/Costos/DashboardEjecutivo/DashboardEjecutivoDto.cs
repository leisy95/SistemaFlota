namespace SistemaFlota.DTOs.Costos.DashboardEjecutivo
{
    public class DashboardEjecutivoDto
    {
        // Indicadores generales
        public decimal TotalKg { get; set; }
        public decimal TotalBultos { get; set; }
        public decimal TotalValor { get; set; }
        public int TotalProveedores { get; set; }
        public int OrdenesPendientes { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal CostoPromedio { get; set; }

        // Indicadores de compras y recepciones
        public decimal GastoCompras { get; set; }
        public int TotalRecepciones { get; set; }
        public int RecepcionesPendientes { get; set; }
        public decimal KgRecibidos { get; set; }
        public decimal BultosRecibidos { get; set; }
        public int EntregasPendientesInventario { get; set; }

        // Gráficas existentes
        public List<StockProveedorDto> StockPorProveedor { get; set; } = new();
        public List<CategoriaMaterialDto> MaterialesPorCategoria { get; set; } = new();
        public List<DensidadMaterialDto> MaterialesPorDensidad { get; set; } = new();
        public List<ValorProveedorDto> ValorPorProveedor { get; set; } = new();

        // Nuevas gráficas y tablas de costos
        public List<CompraPorMesDto> ComprasPorMes { get; set; } = new();
        public List<MaterialMayorCostoDto> MaterialesMayorCosto { get; set; } = new();

        // Paginación de materiales con mayor valor de compra
        public int TotalMaterialesMayorCosto { get; set; }
        public int PaginaMateriales { get; set; } = 1;
        public int PorPaginaMateriales { get; set; } = 5;
    }

    public class StockProveedorDto
    {
        public string Proveedor { get; set; } = string.Empty;
        public decimal Kg { get; set; }
        public decimal Bultos { get; set; }
        public decimal Valor { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class CategoriaMaterialDto
    {
        public string Categoria { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class DensidadMaterialDto
    {
        public string Densidad { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ValorProveedorDto
    {
        public string Proveedor { get; set; } = string.Empty;
        public decimal Valor { get; set; }
    }

    public class CompraPorMesDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public decimal Kg { get; set; }
    }

    public class MaterialMayorCostoDto
    {
        public string Material { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public decimal Kg { get; set; }
        public decimal CostoKg { get; set; }
        public decimal Valor { get; set; }
    }
}