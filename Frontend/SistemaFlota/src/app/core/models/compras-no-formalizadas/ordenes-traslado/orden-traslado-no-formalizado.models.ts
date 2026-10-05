export interface CrearOrdenTrasladoNoFormalizadaDetalle {
    materialNoFormalizadoId: number | null;
    material: string;
    proveedor: string;
    tipo: string;
    densidad: string;
    color: string;
    cantidadKg: number;
    bultos: number;
}

export interface CrearOrdenTrasladoNoFormalizada {
    destino: string;
    materiales: CrearOrdenTrasladoNoFormalizadaDetalle[];
}

export interface OrdenTrasladoDetalleNoFormalizada {
    id: number;
    materialNoFormalizadoId: number | null;
    material: string;
    proveedor: string;
    tipo: string;
    densidad: string;
    color: string;
    cantidadKg: number;
    bultos: number;
    cantidadVerificadaKg: number | null;
    bultosVerificados: number | null;
    estadoVerificacion: string;
}

export interface OrdenTrasladoNoFormalizada {
    id: number;
    numeroOrden: string;
    fecha: string;
    destino: string;
    estado: string;
    usuarioId: number;
    usuario: string;
    totalKg: number;
    totalBultos: number;
    materiales: OrdenTrasladoDetalleNoFormalizada[];
    fechaVerificacion?: string | null;
    usuarioVerificacionId?: number | null;
    usuarioVerificacion?: string | null;
    fechaConfirmacion?: string | null;
    usuarioConfirmacionId?: number | null;
    usuarioConfirmacion?: string | null;
}

export interface OrdenTrasladoNoFormalizadaPaginado {
    datos: OrdenTrasladoNoFormalizada[];
    totalRegistros: number;
    pagina: number;
    tamanoPagina: number;
    totalPaginas: number;
}

export interface VerificarMaterialTrasladoNoFormalizada {
    detalleId: number;
    cantidadVerificadaKg: number;
    bultosVerificados: number;
}

export interface VerificarOrdenTrasladoNoFormalizada {
    ordenTrasladoNoFormalizadaId: number;
    observaciones?: string | null;
    materiales: VerificarMaterialTrasladoNoFormalizada[];
}