export interface Inventario_Costos {
    id: number;
    materialId: number;
    material: string;
    proveedor: string;
    tipo: string;
    color: string;
    densidad: string;
    stockActual: number;
    costoPromedio: number;
    valorInventario: number;
}