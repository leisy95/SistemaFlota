export interface AjusteInventario {
    id: number;
    numeroAjuste: string;
    fecha: Date;
    material: string;
    color: string;
    tipo: string;
    cantidad: number;
    stockAnterior: number;
    stockNuevo: number;
    motivo: string;
    observaciones?: string;
}