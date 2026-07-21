export interface ProveedorFiltro {
    idProveedor: number;
    nombre: string;
}

export interface FiltrosMaterial {
    proveedores: ProveedorFiltro[];
    colores: string[];
}