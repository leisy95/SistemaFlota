import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { Inventario_Costos } from '../../../models/costos/inventario/inventario-costos.models';

@Injectable({
    providedIn: 'root'
})
export class InventarioService {

    private apiUrl = `${environment.apiUrl}/Inventario`;

    constructor(private http: HttpClient) { }

    obtener(
        search?: string,
        proveedorId?: number | null,
        categoria?: string | null,
        color?: string | null,
        page: number = 1,
        pageSize: number = 20
    ): Observable<any> {

        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);

        if (search) params = params.set('search', search);
        if (proveedorId) params = params.set('proveedorId', proveedorId);
        if (categoria) params = params.set('categoria', categoria);
        if (color) params = params.set('color', color);

        return this.http.get<any>(this.apiUrl, { params });
    }

    obtenerProveedores() {
        return this.http.get<any[]>(`${this.apiUrl}/proveedores`);
    }

    obtenerCategorias(): Observable<string[]> {
        return this.http.get<string[]>(`${this.apiUrl}/categorias`);
    }

    exportarExcel(
        search?: string,
        proveedorId?: number | null,
        categoria?: string | null,
        color?: string | null
    ) {

        let params = new HttpParams();

        if (search) params = params.set('search', search);
        if (proveedorId) params = params.set('proveedorId', proveedorId);
        if (categoria) params = params.set('categoria', categoria);
        if (color) params = params.set('color', color);

        return this.http.get(`${this.apiUrl}/excel`, {
            params,
            responseType: 'blob'
        });

    }
}