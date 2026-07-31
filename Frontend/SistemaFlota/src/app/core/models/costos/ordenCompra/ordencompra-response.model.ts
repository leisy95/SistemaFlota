export interface OrdenCompraResponse {
    id: number;
    numero: string;
    proveedorId: number;
    proveedor: string;
    estado: string;
    formaPago: string;
    fechaOrden: string;
    fechaEntrega: string;
    lugarEntrega: string;
    subtotal: number;
    tipoImpuesto: string;
    porcentajeImpuesto: number;
    valorImpuesto: number;
    totalPagar: number;
    observaciones: string;
    usuarioCreacion: string;
    fechaCreacion: string;
    usuarioActualizacion?: string;
    fechaActualizacion?: string;
    detalles: DetalleOrdenResponse[];
}

export interface DetalleOrdenResponse {
    id: number;
    materialId: number;
    material: string;
    color: string;
    cantidadKg: number;
    kgPorBulto: number;
    bultos: number;
    costoKg: number;
    subtotal: number;
}