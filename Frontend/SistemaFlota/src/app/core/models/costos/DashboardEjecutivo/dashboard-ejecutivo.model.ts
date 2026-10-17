
export interface DashboardEjecutivoDto {
    // Indicadores generales
    totalKg: number;
    totalBultos: number;
    totalValor: number;
    totalProveedores: number;
    ordenesPendientes: number;
    totalOrdenes: number;
    costoPromedio: number;

    // Compras y recepciones
    gastoCompras: number;
    totalRecepciones: number;
    recepcionesPendientes: number;
    kgRecibidos: number;
    bultosRecibidos: number;
    entregasPendientesInventario: number;

    // Gráficas actuales
    stockPorProveedor: StockProveedorDto[];
    materialesPorCategoria: CategoriaMaterialDto[];
    materialesPorDensidad: DensidadMaterialDto[];
    valorPorProveedor: ValorProveedorDto[];

    // Nuevas gráficas
    comprasPorMes: CompraPorMesDto[];
    materialesMayorCosto: MaterialMayorCostoDto[];

    // Paginación de materiales con mayor valor de compra
    totalMaterialesMayorCosto: number;
    paginaMateriales: number;
    porPaginaMateriales: number;
}

export interface StockProveedorDto {
    proveedor: string;
    kg: number;
    bultos: number;
    valor: number;
    porcentaje: number;
}

export interface CategoriaMaterialDto {
    categoria: string;
    cantidad: number;
}

export interface DensidadMaterialDto {
    densidad: string;
    cantidad: number;
}

export interface ValorProveedorDto {
    proveedor: string;
    valor: number;
}

export interface CompraPorMesDto {
    mes: string;
    valor: number;
    kg: number;
}

export interface MaterialMayorCostoDto {
    material: string;
    proveedor: string;
    kg: number;
    costoKg: number;
    valor: number;
}
