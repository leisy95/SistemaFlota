import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { CrearOrdenTraslado, OrdenTraslado, OrdenTrasladoPaginado, VerificarOrdenTraslado } from '../../../models/costos/OrdenesTraslado/orden-traslado.model';

@Injectable({
    providedIn: 'root'
})
export class OrdenTrasladoService {

    private readonly apiUrl = `${environment.apiUrl}/OrdenTraslado`;

    constructor(private http: HttpClient) { }

    crear(dto: CrearOrdenTraslado): Observable<OrdenTraslado> {
        return this.http.post<OrdenTraslado>(
            this.apiUrl,
            dto
        );
    }

    obtenerPorId(id: number): Observable<OrdenTraslado> {
        return this.http.get<OrdenTraslado>(
            `${this.apiUrl}/${id}`
        );
    }

    obtenerTodos(
        search: string = '',
        estado: string = '',
        destino: string = '',
        fechaInicio: string = '',
        fechaFin: string = '',
        pagina: number = 1,
        tamanoPagina: number = 10
    ): Observable<OrdenTrasladoPaginado> {

        let params = new HttpParams()
            .set('pagina', pagina)
            .set('tamanoPagina', tamanoPagina);

        if (search.trim()) {
            params = params.set('search', search.trim());
        }

        if (estado) {
            params = params.set('estado', estado);
        }

        if (destino) {
            params = params.set('destino', destino);
        }

        if (fechaInicio) {
            params = params.set('fechaInicio', fechaInicio);
        }

        if (fechaFin) {
            params = params.set('fechaFin', fechaFin);
        }

        return this.http.get<OrdenTrasladoPaginado>(
            this.apiUrl,
            { params }
        );
    }

    verificar(dto: VerificarOrdenTraslado): Observable<OrdenTraslado> {
        return this.http.put<OrdenTraslado>(
            `${this.apiUrl}/verificar`,
            dto
        );
    }

    confirmar(id: number): Observable<OrdenTraslado> {
        return this.http.put<OrdenTraslado>(
            `${this.apiUrl}/${id}/confirmar`,
            {}
        );
    }
}