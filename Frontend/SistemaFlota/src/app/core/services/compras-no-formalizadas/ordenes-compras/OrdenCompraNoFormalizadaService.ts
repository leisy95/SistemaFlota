import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { OrdenCompraNoFormalizadaResponse } from '../../../models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-response.model';
import { CrearOrdenCompraNoFormalizadaRequest } from '../../../models/compras-no-formalizadas/ordenes-compras/crearordencompra-no-formalizada.model';
import { OrdenCompraNoFormalizadaPaginada } from '../../../models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-paginado.model';
import { ActualizarOrdenCompraNoFormalizadaRequest } from '../../../models/compras-no-formalizadas/ordenes-compras/actualizarordencompra-no-formalizada.model';
import { FiltrosOrdenCompraNoFormalizada } from '../../../models/compras-no-formalizadas/ordenes-compras/filtrosordencompra-no-formalizada.model';

@Injectable({
    providedIn: 'root'
})
export class OrdenCompraNoFormalizadaService {

    private api = `${environment.apiUrl}/OrdenesCompraNoFormalizadas`;

    private http = inject(HttpClient);

    obtener(
        page = 1,
        pageSize = 10,
        search = '',
        estado = '',
        proveedorNoFormalizadoId?: number,
        formaPago = '',
        fechaInicio?: string,
        fechaFin?: string
    ) {
        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize)
            .set('_t', Date.now().toString());

        if (search)
            params = params.set('search', search);

        if (estado)
            params = params.set('estado', estado);

        if (proveedorNoFormalizadoId)
            params = params.set('proveedorNoFormalizadoId', proveedorNoFormalizadoId);

        if (formaPago)
            params = params.set('formaPago', formaPago);

        if (fechaInicio)
            params = params.set('fechaInicio', fechaInicio);

        if (fechaFin)
            params = params.set('fechaFin', fechaFin);

        return this.http.get<OrdenCompraNoFormalizadaPaginada>(
            this.api,
            { params }
        );
    }

    // Para mostrar las órdenes de compra en recepción de mercancía
    obtenerParaRecepcion(
        search = '',
        estado = '',
        proveedorNoFormalizadoId?: number,
        page = 1,
        pageSize = 10
    ) {
        let params = new HttpParams()
            .set('page', page)
            .set('pageSize', pageSize)
            .set('_t', Date.now().toString());

        if (search)
            params = params.set('search', search);

        if (estado)
            params = params.set('estado', estado);

        if (proveedorNoFormalizadoId)
            params = params.set('proveedorNoFormalizadoId', proveedorNoFormalizadoId);

        return this.http.get<OrdenCompraNoFormalizadaPaginada>(
            `${this.api}/para-recepcion`,
            { params }
        );
    }

    obtenerPorId(id: number) {
        return this.http.get<OrdenCompraNoFormalizadaResponse>(
            `${this.api}/${id}`
        );
    }

    obtenerFiltros() {
        return this.http.get<FiltrosOrdenCompraNoFormalizada>(
            `${this.api}/filtros`
        );
    }

    crear(data: CrearOrdenCompraNoFormalizadaRequest): Observable<OrdenCompraNoFormalizadaResponse> {
        return this.http.post<OrdenCompraNoFormalizadaResponse>(
            this.api,
            data
        );
    }

    actualizar(
        id: number,
        modelo: ActualizarOrdenCompraNoFormalizadaRequest
    ): Observable<boolean> {
        return this.http.put<boolean>(
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

    enviarCorreo(id: number): Observable<any> {
        return this.http.post<any>(
            `${this.api}/${id}/enviar-correo`,
            {}
        );
    }
}