export interface CrearAjusteInventarioNoFormalizado {
    inventarioId: number;
    tipo: string;
    cantidad: number;
    motivo: string;
    observaciones?: string;
}