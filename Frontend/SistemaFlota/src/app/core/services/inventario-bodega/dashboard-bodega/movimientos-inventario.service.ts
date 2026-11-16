
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { MovimientoInventarioAuditoriaCompleta, MovimientoInventarioRequest, MovimientoInventarioResponse } from '../../../models/inventario-bodega/dashboard-bodega/movimientos-inventario.model';


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

    // Obtener auditoría con filtros de fechas
    obtenerAuditoria(
        fechaInicio?: string,
        fechaFin?: string
    ): Observable<MovimientoInventarioAuditoriaCompleta> {

        let params = new HttpParams();

        if (fechaInicio) {
            params = params.set('fechaInicio', fechaInicio);
        }

        if (fechaFin) {
            params = params.set('fechaFin', fechaFin);
        }

        return this.http.get<MovimientoInventarioAuditoriaCompleta>(
            `${this.apiUrl}/auditoria`,
            { params }
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
