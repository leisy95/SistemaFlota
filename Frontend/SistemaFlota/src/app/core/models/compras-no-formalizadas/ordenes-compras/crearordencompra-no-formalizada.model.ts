export interface CrearOrdenCompraDetalleNoFormalizadaRequest {
    materialNoFormalizadoId: number;
    color: string;
    cantidadKg: number;
    kgPorBulto: number;
    costoKg: number;
}

export interface CrearOrdenCompraNoFormalizadaRequest {
    id?: number;
    proveedorNoFormalizadoId: number;
    fechaOrden: string;
    fechaEntrega: string;
    formaPago: string;
    lugarEntrega: string;
    tipoImpuesto: string;
    porcentajeImpuesto: number;
    observaciones?: string;
    detalles: CrearOrdenCompraDetalleNoFormalizadaRequest[];
}