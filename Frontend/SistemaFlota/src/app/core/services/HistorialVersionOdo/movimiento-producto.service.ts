import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

import { Paginacion } from '../../models/HistorialVersionOdo/paginacion.model';
import { MovimientoProductoModel } from '../../models/HistorialVersionOdo/movimiento-producto.model';

export interface FiltrosMovimientoProducto {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
    producto?: string;
    proveedor?: string;
    estado?: string;
    unidadMedida?: string;
    fechaDesde?: string;
    fechaHasta?: string;
}

@Injectable({
    providedIn: 'root'
})
export class MovimientoProductoService {

    private apiUrl = `${environment.apiUrl}/MovimientosProducto`;

    constructor(private http: HttpClient) { }

    obtener(
        filtros: FiltrosMovimientoProducto = {}
    ): Observable<Paginacion<MovimientoProductoModel>> {

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

        if (filtros.producto?.trim()) {
            params = params.set('producto', filtros.producto.trim());
        }

        if (filtros.proveedor?.trim()) {
            params = params.set('proveedor', filtros.proveedor.trim());
        }

        if (filtros.estado?.trim()) {
            params = params.set('estado', filtros.estado.trim());
        }

        if (filtros.unidadMedida?.trim()) {
            params = params.set('unidadMedida', filtros.unidadMedida.trim());
        }

        if (filtros.fechaDesde) {
            params = params.set('fechaDesde', filtros.fechaDesde);
        }

        if (filtros.fechaHasta) {
            params = params.set('fechaHasta', filtros.fechaHasta);
        }

        return this.http.get<Paginacion<MovimientoProductoModel>>(
            this.apiUrl,
            { params }
        );
    }

    obtenerPorId(id: number): Observable<MovimientoProductoModel> {
        return this.http.get<MovimientoProductoModel>(
            `${this.apiUrl}/${id}`
        );
    }
}