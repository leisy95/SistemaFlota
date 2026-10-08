export interface PedidoCompraModel {
    id: number;
    prioridad: string;
    referenciaOrden: string;
    proveedor: string;
    comprador: string;
    fechaLimiteOrden: string;
    total: number;
    estado: string;
}