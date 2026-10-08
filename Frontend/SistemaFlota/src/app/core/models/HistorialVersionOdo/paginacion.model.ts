export interface Paginacion<T> {
    datos: T[];
    pagina: number;
    porPagina: number;
    totalRegistros: number;
    totalPaginas: number;
}