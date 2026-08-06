import { Inventario_Costos } from "./inventario-costos.models";

export interface InventarioPaginado {
    items: Inventario_Costos[];
    total: number;
    pagina: number;
    pageSize: number;
}