import { OrdenCompraNoFormalizadaResponse } from './ordencompra-no-formalizada-response.model';

export interface OrdenCompraNoFormalizadaPaginada {
    items: OrdenCompraNoFormalizadaResponse[];
    total: number;
    pagina: number;
    pageSize: number;
    totalPaginas: number;
}