
export interface ClienteBodega {
    id: number;
    nombre: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
}

export interface ClienteRequestDto {
    nombre: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
}

export interface EditarClienteRequestDto {
    nombre: string;
    direccion: string | null;
    telefono: string | null;
    email: string | null;
}

export interface RespuestaClienteDto {
    exito: boolean;
    mensaje: string;
    id?: number;
}

// FILTROS DE CLIENTES
export interface ClienteBodegaFiltro {
    buscar: string;
    nombre: string;
    telefono: string;
    email: string;
    pagina: number;
    pageSize: number;
}

// RESPUESTA PAGINADA
export interface ClientesBodegaPaginados {
    items: ClienteBodega[];
    total: number;
    pagina: number;
    pageSize: number;
    totalPaginas: number;
}
