export interface InventarioBodega {
    id: number;
    referencia: string;
    referenciaNormalizada: string;

    descripcion?: string | null;
    tipoBolsa?: string | null;
    tipoMaterial?: string | null;
    densidad?: string | null;
    unidad?: string | null;

    color?: string | null;
    segundoColor?: string | null;
    impresoNo?: string | null;

    ancho: number;
    alto: number;
    calibre: number;

    peso: number;
    cantidad: number;

    fuelleLateral: number;
    fuelleFondo: number;
    fuelleSuperior: number;

    troquel?: string | null;
    referenciaImpresa?: string | null;

    paqueteNumero: number;
    paqueteCantidad: number;
}

// REGISTRAR PRODUCTO
export interface RegistrarItemDto {
    referencia: string;

    descripcion?: string | null;
    tipoBolsa?: string | null;
    tipoMaterial?: string | null;
    densidad?: string | null;

    color?: string | null;
    segundoColor?: string | null;
    impresoNo?: string | null;

    fuelleLateral?: number | null;
    fuelleFondo?: number | null;
    fuelleSuperior?: number | null;

    ancho?: number | null;
    alto?: number | null;
    calibre?: number | null;

    troquel?: string | null;
    referenciaImpresa?: string | null;

    paqueteNumero?: number | null;
    paqueteCantidad?: number | null;
}

// RESPUESTA REGISTRAR
export interface RegistrarItemResponseDto {
    idItemGenerado: number;
    referencia: string;
    message: string;
}

// ACTUALIZAR INVENTARIO
export interface ActualizarInventarioBodegaDto {
    referencia: string;

    descripcion?: string | null;
    tipoBolsa?: string | null;
    tipoMaterial?: string | null;
    densidad?: string | null;

    color?: string | null;
    segundoColor?: string | null;
    impresoNo?: string | null;

    ancho: number;
    alto: number;
    calibre: number;

    peso: number;
    cantidad: number;

    fuelleLateral: number;
    fuelleFondo: number;
    fuelleSuperior: number;

    troquel?: string | null;
    referenciaImpresa?: string | null;

    paqueteNumero: number;
    paqueteCantidad: number;
}

// INGRESAR PESO
export interface IngresarPesoDto {
    referencia: string;
    peso: number;
}

// RESPUESTA INGRESAR PESO
export interface IngresarPesoResponseDto {
    referencia: string;
    referenciaPeso: string;

    peso: number;
    cantidad: number;

    descripcion?: string | null;

    ancho: number;
    alto: number;
    calibre: number;

    color?: string | null;
    material?: string | null;

    fuelleLateral: number;
    fuelleFondo: number;
    fuelleSuperior: number;

    troquel?: string | null;
    referenciaImpresa?: string | null;

    paqueteNumero: number;
    paqueteCantidad: number;
}

export interface RecalcularInventarioBodegaResponseDto {
    message: string;
    totalInventarios: number;
    inventariosActualizados: number;
}