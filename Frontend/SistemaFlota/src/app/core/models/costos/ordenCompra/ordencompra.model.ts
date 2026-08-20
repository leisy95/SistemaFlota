import { DetalleOrdenResponse } from "./ordencompra-response.model";

export interface OrdenCompra {

    id: number;
    numero: string;
    proveedorId: number;
    proveedor: string;
    fechaOrden: string;
    fechaEntrega: string;
    formaPago: string;
    totalItems: number;
    totalKg: number;
    totalBultos: number;
    kgRecibidos: number;
    bultosRecibidos: number;
    kgPendientes: number;
    bultosPendientes: number;
    totalPagar: number;
    estado: string;
    observaciones?: string;

    detalles?: DetalleOrdenResponse[];
}