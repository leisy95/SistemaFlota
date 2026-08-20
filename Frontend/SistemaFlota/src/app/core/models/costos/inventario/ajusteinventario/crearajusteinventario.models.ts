export interface CrearAjusteInventario {
    inventarioId: number;
    tipo: string;
    cantidad: number;
    motivo: string;
    observaciones?: string;
}