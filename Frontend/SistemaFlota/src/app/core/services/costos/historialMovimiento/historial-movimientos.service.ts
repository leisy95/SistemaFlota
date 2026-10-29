
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../../../environments/environment';

import { MovimientoHistorialPaginado } from '../../../models/costos/historialMovimiento/movimiento-historial-paginado.model';
import { MovimientoHistorialResumen } from '../../../models/costos/historialMovimiento/movimiento-historial-resumen.model';
import { MovimientoHistorial } from '../../../models/costos/historialMovimiento/movimiento-historial.model';

export interface FiltrosHistorialMovimientos {
    desde?: string | null;
    hasta?: string | null;
    modulo?: string | null;
    accion?: string | null;
    usuario?: string | null;
    buscar?: string | null;
    pagina?: number;
    porPagina?: number;
}

export interface OpcionesHistorialMovimientos {
    modulos: string[];
    usuarios: string[];
}

@Injectable({
    providedIn: 'root'
})
export class HistorialMovimientosService {

    private readonly http = inject(HttpClient);

    private readonly apiUrl =
        `${environment.apiUrl}/HistorialMovimientos`;

    obtener(
        filtros: FiltrosHistorialMovimientos = {}
    ): Observable<MovimientoHistorialPaginado> {

        const params = this.crearParametros(filtros, true);

        return this.http.get<MovimientoHistorialPaginado>(
            this.apiUrl,
            { params }
        );
    }

    obtenerResumen(
        filtros: FiltrosHistorialMovimientos = {}
    ): Observable<MovimientoHistorialResumen> {

        const params = this.crearParametros(filtros, false);

        return this.http.get<MovimientoHistorialResumen>(
            `${this.apiUrl}/resumen`,
            { params }
        );
    }

    obtenerPorId(id: number): Observable<MovimientoHistorial> {
        return this.http.get<MovimientoHistorial>(
            `${this.apiUrl}/${id}`
        );
    }

    obtenerOpciones(): Observable<OpcionesHistorialMovimientos> {
        return this.http.get<OpcionesHistorialMovimientos>(
            `${this.apiUrl}/opciones`
        );
    }

    private crearParametros(
        filtros: FiltrosHistorialMovimientos,
        incluirPaginacion: boolean
    ): HttpParams {

        let params = new HttpParams();

        if (filtros.desde) {
            params = params.set('desde', filtros.desde);
        }

        if (filtros.hasta) {
            params = params.set('hasta', filtros.hasta);
        }

        if (filtros.modulo?.trim()) {
            params = params.set('modulo', filtros.modulo.trim());
        }

        if (filtros.accion?.trim()) {
            params = params.set('accion', filtros.accion.trim());
        }

        if (filtros.usuario?.trim()) {
            params = params.set('usuario', filtros.usuario.trim());
        }

        if (filtros.buscar?.trim()) {
            params = params.set('buscar', filtros.buscar.trim());
        }

        if (incluirPaginacion) {
            params = params
                .set('pagina', filtros.pagina ?? 1)
                .set('porPagina', filtros.porPagina ?? 10);
        }

        return params;
    }
}
