export interface InventarioCorte {
    materialId: number;
    proveedor: string;
    material: string;
    color: string;
    sistema: number;
    conteo: number;
    diferencia: number;
}

export interface MaterialFiltroCorte {
    materialId: number;
    material: string;
    proveedor: string;
}

export interface FiltrosCorteInventario {
    proveedores: string[];
    materiales: MaterialFiltroCorte[];
}