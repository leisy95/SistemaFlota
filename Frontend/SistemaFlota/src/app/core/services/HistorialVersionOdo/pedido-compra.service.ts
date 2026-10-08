import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';

import { Paginacion } from '../../models/HistorialVersionOdo/paginacion.model';
import { PedidoCompraModel } from '../../models/HistorialVersionOdo/pedido-compra.model';

export interface FiltrosPedidoCompra {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
    prioridad?: string;
    estado?: string;
    fechaDesde?: string;
    fechaHasta?: string;
}

@Injectable({
    providedIn: 'root'
})
export class PedidoCompraService {

    private apiUrl = `${environment.apiUrl}/PedidosCompra`;

    constructor(private http: HttpClient) { }

    obtener(
        filtros: FiltrosPedidoCompra = {}
    ): Observable<Paginacion<PedidoCompraModel>> {

        let params = new HttpParams();

        if (filtros.pagina !== undefined) {
            params = params.set('pagina', filtros.pagina);
        }

        if (filtros.porPagina !== undefined) {
            params = params.set('porPagina', filtros.porPagina);
        }

        if (filtros.buscar?.trim()) {
            params = params.set('buscar', filtros.buscar.trim());
        }

        if (filtros.prioridad?.trim()) {
            params = params.set('prioridad', filtros.prioridad.trim());
        }

        if (filtros.estado?.trim()) {
            params = params.set('estado', filtros.estado.trim());
        }

        if (filtros.fechaDesde) {
            params = params.set('fechaDesde', filtros.fechaDesde);
        }

        if (filtros.fechaHasta) {
            params = params.set('fechaHasta', filtros.fechaHasta);
        }

        return this.http.get<Paginacion<PedidoCompraModel>>(
            this.apiUrl,
            { params }
        );
    }

    obtenerPorId(id: number): Observable<PedidoCompraModel> {

        return this.http.get<PedidoCompraModel>(
            `${this.apiUrl}/${id}`
        );
    }
}