export interface FiltrosOrdenCompra {
    estados: string[];
    proveedores: ProveedorOrdenCompraFiltro[];
    formasPago: string[];
}

export interface ProveedorOrdenCompraFiltro {
    id: number;
    nombre: string;
}