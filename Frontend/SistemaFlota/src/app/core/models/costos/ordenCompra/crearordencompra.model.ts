import { CrearOrdenCompraDetalle } from "./crearordencompradetalle.model";

export interface CrearOrdenCompraRequest {

    id?: number;

    proveedorId: number;
    formaPago: string;
    fechaOrden: string;
    fechaEntrega: string;

    lugarEntrega: string,
    tipoImpuesto: string;
    porcentajeImpuesto: number;

    observaciones?: string;
    detalles: CrearOrdenCompraDetalle[];
}