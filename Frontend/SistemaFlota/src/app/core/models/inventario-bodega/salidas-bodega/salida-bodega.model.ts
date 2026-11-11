export interface SalidaBodegaDetalle {
    idInventarioBodegaItem: number;
    referenciaPeso: string;
    pesoActual: number;
}

export interface SalidaBodegaActual {
    existe: boolean;
    mensaje: string;
    id: number;
    estado: string;
    fechaCreacion: string;
    detalles: SalidaBodegaDetalle[];
}