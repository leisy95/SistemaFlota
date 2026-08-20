import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { Proveedor } from '../../../models/costos/proveedores/proveedores.model';
import { CrearProveedor } from '../../../../features/costos/proveedores-materiales/proveedores/crear-proveedor/crear-proveedor';


@Injectable({
    providedIn: 'root'
})
export class ProveedorService {

    private readonly http = inject(HttpClient);

    private readonly apiUrl = `${environment.apiUrl}/Proveedores`;

    // Filtros
    obtener(
        search: string = '',
        estado: string = '',
        orden: string = '',
        page: number = 1,
        pageSize: number = 10
    ): Observable<any> {

        let params = new HttpParams()
            .set('search', search)
            .set('estado', estado)
            .set('orden', orden)
            .set('page', page)
            .set('pageSize', pageSize);

        return this.http.get<any>(this.apiUrl, { params });
    }

    obtenerPorId(id: number): Observable<Proveedor> {
        return this.http.get<Proveedor>(`${this.apiUrl}/${id}`);
    }

    // Crear proveedor
    crear(dto: Proveedor): Observable<Proveedor> {
        return this.http.post<Proveedor>(this.apiUrl, dto);
    }

    // Actualizar Proveedor
    actualizar(id: number, proveedor: Proveedor): Observable<void> {
        return this.http.put<void>(
            `${this.apiUrl}/${id}`,
            proveedor
        );
    }

    eliminar(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }
}