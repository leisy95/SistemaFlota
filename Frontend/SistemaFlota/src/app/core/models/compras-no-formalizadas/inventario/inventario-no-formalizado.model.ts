export interface InventarioNoFormalizado {
    id: number;
    materialId: number;
    material: string;
    proveedor: string;
    categoria: string;
    tipoProduccion: string;
    color: string;
    densidad: string;
    stockActual: number;
    cantidadComprometida: number;
    stockDisponible: number;
    costoPromedio: number;
    valorInventario: number;
}