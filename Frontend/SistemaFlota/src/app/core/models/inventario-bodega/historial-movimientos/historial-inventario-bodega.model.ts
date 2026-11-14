export interface MovimientoInventarioBodega {
    id: number;
    fecha: string;
    usuario: string;
    rol?: string | null;
    modulo: string;
    accion: string;
    detalle?: string | null;
    registroId?: number | null;
    ipAddress?: string | null;
    resultado?: string | null;
}

export interface MovimientoInventarioBodegaPaginado {
    movimientos: MovimientoInventarioBodega[];
    totalRegistros: number;
    pagina: number;
    porPagina: number;
    totalPaginas: number;
}

export interface MovimientoInventarioBodegaResumen {
    totalMovimientos: number;
    totalCreaciones: number;
    totalActualizaciones: number;
    totalEliminaciones: number;
    totalConfirmaciones: number;
    totalCancelaciones: number;
}

export interface MovimientoInventarioBodegaOpciones {
    modulos: string[];
    usuarios: string[];
    acciones: string[];
}

export interface FiltroHistorialInventarioBodega {
    desde?: string | null;
    hasta?: string | null;
    modulo?: string | null;
    accion?: string | null;
    usuario?: string | null;
    buscar?: string | null;
    pagina: number;
    porPagina: number;
}