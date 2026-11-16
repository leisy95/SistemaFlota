
export interface MovimientoInventarioRequest {
    referencia: string;
    referenciaPeso: string;
    peso: number;
    fecha: string;
    tipo: string;
    usuario: string;
    idInventarioBodega: number;
    idInventarioBodegaItem: number;
    clienteId?: number | null;
    observacion?: string | null;
}

export interface MovimientoInventarioResponse {
    id: number;
    referencia: string;
    referenciaPeso: string;
    peso: number;
    fecha: string;
    tipo: string;
    usuario: string;
    referenciaImpresa?: string | null;
    inventarioNombre?: string | null;
    inventarioItemCodigo?: string | null;
}

export interface MovimientoInventarioAuditoria {
    referencia: string;
    referenciaPeso: string;
    tipo: string;
    peso: number;
    fecha: string;
    usuario: string;
    cliente?: string | null;
    referenciaImpresa?: string | null;
    observacion?: string | null;
}

export interface MovimientoInventarioTotales {
    cantidad: number;
    peso: number;
}

export interface MovimientoInventarioAuditoriaResumen {
    inventarioInicial: MovimientoInventarioTotales;
    entradas: MovimientoInventarioTotales;
    salidas: MovimientoInventarioTotales;
    devoluciones: MovimientoInventarioTotales;
    eliminados: MovimientoInventarioTotales;
    ajustes: MovimientoInventarioTotales;
    existencias: MovimientoInventarioTotales;
}

export interface MovimientoInventarioEstadistica {
    tipo: string;
    total: number;
}

export interface ReferenciaMasMovida {
    referencia: string;
    total: number;
}

export interface UsuarioMasActivo {
    usuario: string;
    total: number;
}

export interface MovimientoInventarioAuditoriaCompleta {
    resumen: MovimientoInventarioAuditoriaResumen;
    movimientos: MovimientoInventarioAuditoria[];
    totalesPorTipo: MovimientoInventarioEstadistica[];
    referenciasMasMovidas: ReferenciaMasMovida[];
    usuariosMasActivos: UsuarioMasActivo[];
}
