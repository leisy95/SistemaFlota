import { InventarioNoFormalizado } from './inventario-no-formalizado.model';

export interface InventarioNoFormalizadoPaginado {
    items: InventarioNoFormalizado[];
    total: number;
    pagina: number;
    pageSize: number;
    totalKg: number;
    totalValorInventario: number;
}