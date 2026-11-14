import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';
import { FiltroHistorialInventarioBodega, MovimientoInventarioBodega, MovimientoInventarioBodegaOpciones, MovimientoInventarioBodegaPaginado, MovimientoInventarioBodegaResumen } from '../../../models/inventario-bodega/historial-movimientos/historial-inventario-bodega.model';

@Injectable({
    providedIn: 'root'
})
export class HistorialInventarioBodegaService {

    private http = inject(HttpClient);

    private apiUrl = `${environment.apiUrl}/HistorialInventarioBodega`;

    // OBTENER MOVIMIENTOS PAGINADOS
    obtener(
        filtros: FiltroHistorialInventarioBodega
    ): Observable<MovimientoInventarioBodegaPaginado> {

        let params = new HttpParams()
            .set('pagina', filtros.pagina)
            .set('porPagina', filtros.porPagina);

        if (filtros.desde) {
            params = params.set('desde', filtros.desde);
        }

        if (filtros.hasta) {
            params = params.set('hasta', filtros.hasta);
        }

        if (filtros.modulo) {
            params = params.set('modulo', filtros.modulo);
        }

        if (filtros.accion) {
            params = params.set('accion', filtros.accion);
        }

        if (filtros.usuario) {
            params = params.set('usuario', filtros.usuario);
        }

        if (filtros.buscar) {
            params = params.set('buscar', filtros.buscar);
        }

        return this.http.get<MovimientoInventarioBodegaPaginado>(
            this.apiUrl,
            { params }
        );
    }

    // OBTENER RESUMEN
    obtenerResumen(
        filtros: Omit<FiltroHistorialInventarioBodega, 'pagina' | 'porPagina'>
    ): Observable<MovimientoInventarioBodegaResumen> {

        let params = new HttpParams();

        if (filtros.desde) {
            params = params.set('desde', filtros.desde);
        }

        if (filtros.hasta) {
            params = params.set('hasta', filtros.hasta);
        }

        if (filtros.modulo) {
            params = params.set('modulo', filtros.modulo);
        }

        if (filtros.accion) {
            params = params.set('accion', filtros.accion);
        }

        if (filtros.usuario) {
            params = params.set('usuario', filtros.usuario);
        }

        if (filtros.buscar) {
            params = params.set('buscar', filtros.buscar);
        }

        return this.http.get<MovimientoInventarioBodegaResumen>(
            `${this.apiUrl}/resumen`,
            { params }
        );
    }

    // OBTENER OPCIONES DE FILTROS
    obtenerOpciones(): Observable<MovimientoInventarioBodegaOpciones> {
        return this.http.get<MovimientoInventarioBodegaOpciones>(
            `${this.apiUrl}/opciones`
        );
    }

    // OBTENER MOVIMIENTO POR ID
    obtenerPorId(id: number): Observable<MovimientoInventarioBodega> {
        return this.http.get<MovimientoInventarioBodega>(
            `${this.apiUrl}/${id}`
        );
    }
}