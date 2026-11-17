
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';

import {
    MovimientoInventarioAuditoriaCompleta,
    MovimientoInventarioFiltros,
    MovimientoInventarioFiltrosOpciones,
    MovimientoInventarioRequest,
    MovimientoInventarioResponse
} from '../../../models/inventario-bodega/dashboard-bodega/movimientos-inventario.model';

@Injectable({
    providedIn: 'root'
})
export class MovimientosInventarioService {

    private apiUrl = `${environment.apiUrl}/MovimientosInventario`;

    constructor(private http: HttpClient) { }

    // Obtener todos los movimientos
    obtenerMovimientos(): Observable<MovimientoInventarioResponse[]> {
        return this.http.get<MovimientoInventarioResponse[]>(this.apiUrl);
    }

    // Obtener movimiento por ID
    obtenerMovimientoPorId(id: number): Observable<MovimientoInventarioResponse> {
        return this.http.get<MovimientoInventarioResponse>(`${this.apiUrl}/${id}`);
    }

    // Obtener auditoría con filtros y paginación desde el backend
    obtenerAuditoria(
        filtros: MovimientoInventarioFiltros
    ): Observable<MovimientoInventarioAuditoriaCompleta> {

        let params = new HttpParams()
            .set('pagina', (filtros.pagina ?? 1).toString())
            .set('pageSize', (filtros.pageSize ?? 10).toString());

        if (filtros.fechaInicio) {
            params = params.set('fechaInicio', filtros.fechaInicio);
        }

        if (filtros.fechaFin) {
            params = params.set('fechaFin', filtros.fechaFin);
        }

        if (filtros.referencia?.trim()) {
            params = params.set('referencia', filtros.referencia.trim());
        }

        if (filtros.usuario?.trim()) {
            params = params.set('usuario', filtros.usuario.trim());
        }

        if (filtros.cliente?.trim()) {
            params = params.set('cliente', filtros.cliente.trim());
        }

        if (filtros.tipo?.trim()) {
            params = params.set('tipo', filtros.tipo.trim());
        }

        if (filtros.referenciaImpresa?.trim()) {
            params = params.set('referenciaImpresa', filtros.referenciaImpresa.trim());
        }

        return this.http.get<MovimientoInventarioAuditoriaCompleta>(
            `${this.apiUrl}/auditoria`,
            { params }
        );
    }

    // Para filtros
    obtenerOpcionesFiltros() {
        return this.http.get<MovimientoInventarioFiltrosOpciones>(
            `${this.apiUrl}/filtros/opciones`
        );
    }

    // Generar PDF de movimientos
    generarPdf(
        filtros: MovimientoInventarioFiltros
    ): Observable<Blob> {
        return this.http.post(
            `${this.apiUrl}/pdf`,
            filtros,
            { responseType: 'blob' }
        );
    }

    // Registrar movimiento
    registrarMovimiento(
        movimiento: MovimientoInventarioRequest
    ): Observable<MovimientoInventarioResponse> {
        return this.http.post<MovimientoInventarioResponse>(
            this.apiUrl,
            movimiento
        );
    }
}
