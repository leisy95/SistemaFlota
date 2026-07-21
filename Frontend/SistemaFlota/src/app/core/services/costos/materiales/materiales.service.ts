import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { MaterialPaginado } from '../../../models/materiales/material.pag.models';
import { Material } from '../../../models/materiales/material.models';
import { FiltrosMaterial } from '../../../models/materiales/filtros-material.models';

@Injectable({
    providedIn: 'root'
})
export class MaterialService {

    private readonly http = inject(HttpClient);

    private readonly apiUrl = `${environment.apiUrl}/Materiales`;

    obtener(
        search: string = '',
        estado: string = '',
        orden: string = '',
        proveedor: string = '',
        color: string = '',
        page: number = 1,
        pageSize: number = 10
    ): Observable<MaterialPaginado> {

        const params = new HttpParams()
            .set('search', search)
            .set('estado', estado)
            .set('orden', orden)
            .set('proveedor', proveedor)
            .set('color', color)
            .set('page', page)
            .set('pageSize', pageSize);

        return this.http.get<MaterialPaginado>(this.apiUrl, { params });
    }


    crear(material: Material): Observable<Material> {
        return this.http.post<Material>(
            this.apiUrl,
            material
        );
    }


    obtenerPorId(id: number): Observable<Material> {

        return this.http.get<Material>(
            `${this.apiUrl}/${id}`
        );
    }

    actualizar(id: number, material: Material): Observable<void> {

        return this.http.put<void>(
            `${this.apiUrl}/${id}`,
            material
        );
    }

    obtenerFiltros(): Observable<FiltrosMaterial> {
        return this.http.get<FiltrosMaterial>(
            `${this.apiUrl}/filtros`
        );
    }


    eliminar(id: number): Observable<void> {

        return this.http.delete<void>(
            `${this.apiUrl}/${id}`
        );
    }
}