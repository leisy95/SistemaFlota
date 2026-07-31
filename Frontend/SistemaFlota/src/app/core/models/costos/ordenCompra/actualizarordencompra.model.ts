import { ActualizarOrdenCompraDetalle } from "./actualizarordencompradetalle.model";

export interface ActualizarOrdenCompra {

    proveedorId: number;
    fechaOrden: Date;
    fechaEntrega: Date;
    formaPago: string;
    observaciones?: string;
    detalles: ActualizarOrdenCompraDetalle[];

}