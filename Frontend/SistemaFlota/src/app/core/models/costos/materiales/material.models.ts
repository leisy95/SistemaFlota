export interface Material {
    idMaterial?: number;
    idProveedor: number;
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
    activo: boolean;
    documentoPdf?: string;
    fechaCreacion?: Date;
    fechaActualizacion?: Date;
}