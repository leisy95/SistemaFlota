
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { DashboardEjecutivoDto } from '../../../models/costos/DashboardEjecutivo/dashboard-ejecutivo.model';

export interface FiltrosDashboardEjecutivo {
    fechaDesde?: string;
    fechaHasta?: string;
    proveedor?: string;
    estado?: string;
    categoria?: string;
    densidad?: string;
}

export interface OpcionesFiltrosDashboard {
    estados: string[];
    categorias: string[];
    densidades: string[];
    proveedores: string[];
}

@Injectable({
    providedIn: 'root'
})
export class DashboardEjecutivoService {
    private readonly http = inject(HttpClient);

    private readonly apiUrl =
        `${environment.apiUrl}/DashboardEjecutivo`;

    obtenerDashboard(
        pagina: number = 1,
        porPagina: number = 5,
        filtros?: FiltrosDashboardEjecutivo
    ): Observable<DashboardEjecutivoDto> {

        let params = new HttpParams()
            .set('pagina', pagina)
            .set('porPagina', porPagina);

        if (filtros?.fechaDesde) {
            params = params.set('fechaDesde', filtros.fechaDesde);
        }

        if (filtros?.fechaHasta) {
            params = params.set('fechaHasta', filtros.fechaHasta);
        }

        if (filtros?.proveedor) {
            params = params.set('proveedor', filtros.proveedor);
        }

        if (filtros?.estado) {
            params = params.set('estado', filtros.estado);
        }

        if (filtros?.categoria) {
            params = params.set('categoria', filtros.categoria);
        }

        if (filtros?.densidad) {
            params = params.set('densidad', filtros.densidad);
        }

        return this.http.get<DashboardEjecutivoDto>(
            this.apiUrl,
            { params }
        );
    }

    // Consulta las opciones reales de los filtros.
    obtenerFiltros(): Observable<OpcionesFiltrosDashboard> {
        return this.http.get<OpcionesFiltrosDashboard>(
            `${this.apiUrl}/filtros`
        );
    }
}
