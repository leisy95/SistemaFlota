export interface Material {
    idMaterial?: number;
    idProveedor: number;
    proveedor?: string;
    nombreMaterial: string;
    descripcionCompra?: string;
    densidad: string;
    categoria: string;
    color?: string;
    tipoProduccion?: string;
    unidad: string;
    precioBaseKg: number;
    activo: boolean;
    fechaCreacion?: Date;
    fechaActualizacion?: Date;
}