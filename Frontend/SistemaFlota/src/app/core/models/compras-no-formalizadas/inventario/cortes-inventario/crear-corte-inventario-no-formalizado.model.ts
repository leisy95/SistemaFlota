export interface CrearCorteInventarioNoFormalizado {
    detalles: DetalleCorteInventarioNoFormalizado[];
}

export interface DetalleCorteInventarioNoFormalizado {
    materialId: number;
    color: string | null;
    conteo: number;
}