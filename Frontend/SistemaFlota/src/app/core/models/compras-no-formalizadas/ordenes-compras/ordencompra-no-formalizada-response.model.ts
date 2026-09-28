export interface OrdenCompraNoFormalizadaResponse {
    id: number;
    numero: string;
    proveedorNoFormalizadoId: number;
    proveedor: string;
    fechaOrden: string;
    fechaEntrega: string;
    formaPago: string;
    lugarEntrega: string;
    totalItems: number;
    totalKg: number;
    totalBultos: number;
    subtotal: number;
    tipoImpuesto: string;
    porcentajeImpuesto: number;
    valorImpuesto: number;
    totalPagar: number;
    estado: string;
    observaciones?: string;
    recepcionId?: number;
    kgRecibidos: number;
    bultosRecibidos: number;
    kgPendientes: number;
    bultosPendientes: number;
    usuarioCreacion: string;
    fechaCreacion: string;
    usuarioActualizacion: string;
    fechaActualizacion?: string;
    detalles: OrdenCompraDetalleNoFormalizadaResponse[];
}

export interface OrdenCompraDetalleNoFormalizadaResponse {
    id: number;
    materialNoFormalizadoId: number;
    material: string;
    color: string;
    cantidadKg: number;
    kgPorBulto: number;
    bultos: number;
    costoKg: number;
    subtotal: number;
}