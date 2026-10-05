export interface FiltrosOrdenCompraNoFormalizada {
    estados: string[];
    proveedores: ProveedorOrdenCompraNoFormalizadaFiltro[];
    formasPago: string[];
}

export interface ProveedorOrdenCompraNoFormalizadaFiltro {
    id: number;
    nombre: string;
}