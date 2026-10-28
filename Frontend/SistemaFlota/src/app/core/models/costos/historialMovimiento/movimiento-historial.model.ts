export interface MovimientoHistorial {
    id: number;
    fecha: string;
    usuario: string;
    rol: string;
    modulo: string;
    accion: string;
    detalle: string | null;
    registroId: number | null;
    ipAddress: string | null;
    resultado: string;
    descripcion: string;
}
