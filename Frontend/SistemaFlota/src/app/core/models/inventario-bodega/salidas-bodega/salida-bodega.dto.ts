export interface RespuestaSalidaBodegaDto {
    exito: boolean;
    mensaje: string;
    id?: number;
}

export interface ConfirmarSalidaBodegaDto {
    clienteId: number;
}