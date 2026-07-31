import { Injectable, inject } from '@angular/core';

import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { OrdenCompra } from '../../../models/costos/ordenCompra/ordencompra.model';
import { environment } from '../../../../../environments/environment';
import { CrearOrdenCompraRequest } from '../../../models/costos/ordenCompra/crearordencompra.model';
import { OrdenCompraPaginado } from '../../../models/costos/ordenCompra/ordencomprapag.model';
import { ActualizarOrdenCompra } from '../../../models/costos/ordenCompra/actualizarordencompra.model';
import { OrdenCompraResponse } from '../../../models/costos/ordenCompra/ordencompra-response.model';

@Injectable({
    providedIn: 'root'
})
export class OrdenCompraService {

    private api = `${environment.apiUrl}/OrdenCompra`;

    private http = inject(HttpClient);

    obtener(
        page = 1,
        pageSize = 10,
        search = '',
        estado = '',
        proveedorId?: number,
        formaPago = '',
        fechaInicio?: string,
        fechaFin?: string
    ) {

        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize);

        if (search)
            params = params.set('search', search);

        if (estado)
            params = params.set('estado', estado);

        if (proveedorId)
            params = params.set('proveedorId', proveedorId);

        if (formaPago)
            params = params.set('formaPago', formaPago);

        if (fechaInicio)
            params = params.set('fechaInicio', fechaInicio);

        if (fechaFin)
            params = params.set('fechaFin', fechaFin);

        return this.http.get<OrdenCompraPaginado>(this.api, { params });
    }

    obtenerPorId(id: number) {
        return this.http.get<OrdenCompraResponse>(`${this.api}/${id}`);
    }

    crear(data: CrearOrdenCompraRequest): Observable<OrdenCompra> {

        return this.http.post<OrdenCompra>(
            this.api,
            data
        );

    }

    actualizar(id: number, modelo: CrearOrdenCompraRequest): Observable<void> {
        return this.http.put<void>(
            `${this.api}/${id}`,
            modelo
        );
    }

    generarPdf(id: number) {
        return this.http.get(
            `${this.api}/${id}/pdf`,
            {
                responseType: 'blob'
            }
        );
    }
}