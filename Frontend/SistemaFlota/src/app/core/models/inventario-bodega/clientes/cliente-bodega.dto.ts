
export interface ClienteBodegaRequestDto {
    nombre: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
}

export interface EditarClienteBodegaDto {
    id: number;
    nombre: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
}

export interface RespuestaClienteBodegaDto {
    exito: boolean;
    mensaje: string;
    id?: number;
}
