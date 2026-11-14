export interface MostrarEntradaDto {
    id: number;
    referencia: string;
    referenciaPeso: string;
    fechaRegistroItem: string;
    pesoActual: number;
    estado: string;
}

export interface IngresarPesoDto {
    referencia: string;
    peso: number;
}

export interface IngresarPesoResponseDto {
    referencia: string;
    referenciaPeso: string;
    peso: number;
    cantidad: number;
    descripcion?: string;
    ancho?: number;
    alto?: number;
    calibre?: number;
    color?: string;
    material?: string;
    fuelleLateral?: number;
    fuelleFondo?: number;
    fuelleSuperior?: number;
    troquel?: string;
    referenciaImpresa?: string;
    paqueteNumero?: number;
    paqueteCantidad?: number;
}