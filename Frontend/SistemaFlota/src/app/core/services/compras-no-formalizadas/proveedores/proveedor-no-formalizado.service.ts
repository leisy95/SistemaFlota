import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ProveedorNoFormalizado } from '../../../models/compras-no-formalizadas/proveedores/proveedor-no-formalizado.model';

export interface ProveedorNoFormalizadoPaginado {
    totalRegistros: number;
    pagina: number;
    tamanoPagina: number;
    totalPaginas: number;
    datos: ProveedorNoFormalizado[];
}

@Injectable({
    providedIn: 'root'
})
export class ProveedorNoFormalizadoService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/ProveedorNoFormalizado`;

    obtener(
        search: string = '',
        estado: string = '',
        orden: string = '',
        page: number = 1,
        pageSize: number = 10
    ): Observable<ProveedorNoFormalizadoPaginado> {
        const params = new HttpParams()
            .set('search', search)
            .set('estado', estado)
            .set('orden', orden)
            .set('page', page)
            .set('pageSize', pageSize);

        return this.http.get<ProveedorNoFormalizadoPaginado>(
            this.apiUrl,
            { params }
        );
    }

    obtenerPorId(id: number): Observable<ProveedorNoFormalizado> {
        return this.http.get<ProveedorNoFormalizado>(
            `${this.apiUrl}/${id}`
        );
    }

    crear(proveedor: ProveedorNoFormalizado): Observable<ProveedorNoFormalizado> {
        return this.http.post<ProveedorNoFormalizado>(
            this.apiUrl,
            proveedor
        );
    }

    actualizar(
        id: number,
        proveedor: ProveedorNoFormalizado
    ): Observable<void> {
        return this.http.put<void>(
            `${this.apiUrl}/${id}`,
            proveedor
        );
    }
}