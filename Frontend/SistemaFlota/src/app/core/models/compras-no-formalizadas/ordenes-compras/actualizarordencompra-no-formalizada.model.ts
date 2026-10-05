import { CrearOrdenCompraDetalleNoFormalizadaRequest } from './crearordencompra-no-formalizada.model';

export interface ActualizarOrdenCompraNoFormalizadaRequest {
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