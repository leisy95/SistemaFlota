import { OrdenCompra } from "./ordencompra.model";

export interface OrdenCompraPaginado {
    items: OrdenCompra[];
    total: number;
    pagina: number;
    paginas: number;
    pageSize: number;
}