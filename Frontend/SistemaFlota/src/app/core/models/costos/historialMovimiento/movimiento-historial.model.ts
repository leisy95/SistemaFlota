export interface MovimientoHistorial {
    id: number;
    fecha: string | Date;
    usuario: string;
    rol: string | null;
    modulo: string;
    accion: string;
    detalle: string | null;
    registroId: number | null;
    ipAddress: string | null;
    resultado: string;
}