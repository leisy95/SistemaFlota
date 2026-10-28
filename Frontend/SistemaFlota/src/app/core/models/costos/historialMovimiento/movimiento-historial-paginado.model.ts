
import { MovimientoHistorial } from './movimiento-historial.model';

export interface MovimientoHistorialPaginado {
    movimientos: MovimientoHistorial[];
    totalRegistros: number;
    pagina: number;
    porPagina: number;
    totalPaginas: number;
}
