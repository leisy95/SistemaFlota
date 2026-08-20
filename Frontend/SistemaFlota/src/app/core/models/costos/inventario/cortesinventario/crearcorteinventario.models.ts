export interface CrearCorteInventario {
    detalles: DetalleCorteInventario[];
}

export interface DetalleCorteInventario {
    materialId: number;
    conteo: number;
}