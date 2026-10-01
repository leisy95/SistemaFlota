export interface HistorialCorteDetalleNoFormalizado {
    id: number;
    fecha: string;
    estado: string;
    usuario: string;
    detalles: DetalleHistorialCorteNoFormalizado[];
}

export interface DetalleHistorialCorteNoFormalizado {
    materialId: number;
    material: string;
    proveedor: string;
    color: string | null;
    stockSistema: number;
    conteoFisico: number;
    diferencia: number;
}