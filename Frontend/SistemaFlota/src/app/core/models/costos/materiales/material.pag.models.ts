import { Material } from "./material.models";

export interface MaterialPaginado {
    totalRegistros: number;
    pagina: number;
    tamanoPagina: number;
    totalPaginas: number;
    datos: Material[];
}