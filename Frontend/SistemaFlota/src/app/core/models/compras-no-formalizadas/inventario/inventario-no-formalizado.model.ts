export interface InventarioNoFormalizado {
    id: number;
    materialId: number;
    material: string;
    proveedor: string;
    categoria: string | null;
    color: string;
    densidad: string;
    cantidadComprometida: number | null;
    stockDisponible: number | null;
    stockActual: number | null;
    costoPromedio: number | null;
    valorInventario: number | null;
}