import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { DashboardResumenInicio } from '../../../models/Dashboards/dashboard-resumen-inicio/dashboard-resumen-inicio.model';

@Injectable({
    providedIn: 'root'
})
export class DashboardResumenInicioService {

    private apiUrl = `${environment.apiUrl}/DashboardResumenInicio`;

    constructor(private http: HttpClient) { }

    obtenerResumen(): Observable<DashboardResumenInicio> {
        return this.http.get<DashboardResumenInicio>(
            this.apiUrl
        );
    }
}