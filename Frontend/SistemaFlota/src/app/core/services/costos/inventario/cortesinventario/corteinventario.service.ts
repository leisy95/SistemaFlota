import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CrearCorteInventario } from '../../../../models/costos/inventario/cortesinventario/crearcorteinventario.models';
import { FiltrosCorteInventario, InventarioCorte } from '../../../../models/costos/inventario/cortesinventario/corteinventario.models';
import { CorteInventarioHistorial, HistorialCorteDetalle } from '../../../../models/costos/inventario/historialcorteinventario/historialcorteInventario.models';

@Injectable({
    providedIn: 'root'
})
export class CorteInventarioService {

    private apiUrl = `${environment.apiUrl}/CorteInventario`;

    constructor(private http: HttpClient) { }

    obtenerCorte(): Observable<InventarioCorte[]> {
        return this.http.get<InventarioCorte[]>(this.apiUrl);
    }

    guardarCorte(dto: CrearCorteInventario): Observable<any> {
        return this.http.post<any>(this.apiUrl, dto);
    }

    obtenerHistorial(): Observable<CorteInventarioHistorial[]> {
        return this.http.get<CorteInventarioHistorial[]>(
            `${this.apiUrl}/historial`
        );
    }

    obtenerDetalle(id: number): Observable<HistorialCorteDetalle> {
        return this.http.get<HistorialCorteDetalle>(
            `${this.apiUrl}/${id}`
        );
    }

    // filtro
    obtenerFiltros() {
        return this.http.get<FiltrosCorteInventario>(
            `${this.apiUrl}/filtros`
        );
    }

    generarPdf(material: string = '', proveedor: string = ''): Observable<Blob> {
        const params: any = {};

        if (material.trim()) {
            params.material = material.trim();
        }

        if (proveedor.trim()) {
            params.proveedor = proveedor.trim();
        }

        return this.http.get(
            `${this.apiUrl}/pdf`,
            {
                params,
                responseType: 'blob'
            }
        );
    }

}