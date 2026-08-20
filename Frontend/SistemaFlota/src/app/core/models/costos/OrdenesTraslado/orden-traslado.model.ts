export interface CrearOrdenTrasladoDetalle {
    materialId: number | null;
    proveedor: string;
    tipo: string;
    densidad: string;
    color: string;
    cantidadKg: number;
    bultos: number;
}

export interface CrearOrdenTraslado {
    destino: string;
    materiales: CrearOrdenTrasladoDetalle[];
}

export interface OrdenTrasladoDetalle {
    id: number;
    materialId: number | null;
    proveedor: string;
    tipo: string;
    densidad: string;
    color: string;
    cantidadKg: number;
    bultos: number;

    cantidadVerificadaKg: number;
    bultosVerificados: number;
    estadoVerificacion: string;
}

export interface OrdenTraslado {
    id: number;
    numeroOrden: string;
    fecha: string;
    destino: string;
    estado: string;
    usuarioId: number;
    usuario: string;
    totalKg: number;
    totalBultos: number;
    materiales: OrdenTrasladoDetalle[];

    fechaVerificacion?: string | null;
    usuarioVerificacionId?: number | null;
    usuarioVerificacion?: string | null;

    fechaConfirmacion?: string | null;
    usuarioConfirmacionId?: number | null;
    usuarioConfirmacion?: string | null;
}

export interface OrdenTrasladoPaginado {
    datos: OrdenTraslado[];
    totalRegistros: number;
    pagina: number;
    tamanoPagina: number;
    totalPaginas: number;
}

export interface VerificarMaterialTraslado {
    detalleId: number;
    cantidadVerificadaKg: number;
    bultosVerificados: number;
}

export interface VerificarOrdenTraslado {
    ordenTrasladoId: number;
    observaciones?: string | null;
    materiales: VerificarMaterialTraslado[];
}