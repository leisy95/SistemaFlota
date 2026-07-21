export interface Material {
    idMaterial?: number;
    idProveedor: number;
    proveedor?: string;
    materiaPrima: string;
    descripcionCompra?: string;
    densidad: string;
    categoria: string;
    color?: string;
    lineaProduccion?: string;
    unidad: string;
    precioBaseKg: number;
    bultos: number;
    cantidadKg: number;
    activo: boolean;
    fechaCreacion?: Date;
    fechaActualizacion?: Date;
}