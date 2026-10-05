export interface MaterialNoFormalizado {
    idMaterialNoFormalizado?: number;
    idProveedorNoFormalizado: number;
    proveedor?: string;
    codigo: string;
    nombreMaterial: string;
    descripcionCompra?: string;
    densidad: string;
    categoria: string;
    color?: string;
    tipoProduccion?: string;
    unidad: string;
    precioBaseKg: number;
    documentoPdf?: string;
    activo: boolean;
    fechaCreacion?: string;
    fechaActualizacion?: string;
}