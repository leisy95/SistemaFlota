export interface CorteInventarioHistorial {
    id: number;
    fecha: string;
    estado: string;
    usuario: string;
    cantidadDetalles: number;
}

export interface DetalleHistorialCorte {
    materialId: number;
    material: string;
    proveedor: string;
    color: string | null;
    stockSistema: number;
    conteoFisico: number;
    diferencia: number;
}

export interface HistorialCorteDetalle {
    id: number;
    fecha: string;
    estado: string;
    usuario: string;
    detalles: DetalleHistorialCorte[];
}