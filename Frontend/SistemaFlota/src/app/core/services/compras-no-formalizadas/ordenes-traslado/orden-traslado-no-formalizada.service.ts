import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { CrearOrdenTrasladoNoFormalizada, OrdenTrasladoNoFormalizada, OrdenTrasladoNoFormalizadaPaginado, VerificarOrdenTrasladoNoFormalizada } from '../../../models/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizado.models';

@Injectable({
    providedIn: 'root'
})
export class OrdenTrasladoNoFormalizadaService {
    private readonly apiUrl = `${environment.apiUrl}/OrdenTrasladoNoFormalizada`;

    constructor(private http: HttpClient) { }

    crear(dto: CrearOrdenTrasladoNoFormalizada): Observable<OrdenTrasladoNoFormalizada> {
        return this.http.post<OrdenTrasladoNoFormalizada>(this.apiUrl, dto);
    }

    obtenerPorId(id: number): Observable<OrdenTrasladoNoFormalizada> {
        return this.http.get<OrdenTrasladoNoFormalizada>(`${this.apiUrl}/${id}`);
    }

    obtenerTodos(
        search: string = '',
        estado: string = '',
        destino: string = '',
        fechaInicio: string = '',
        fechaFin: string = '',
        pagina: number = 1,
        tamanoPagina: number = 10
    ): Observable<OrdenTrasladoNoFormalizadaPaginado> {
        let params = new HttpParams()
            .set('pagina', pagina)
            .set('tamanoPagina', tamanoPagina);

        if (search.trim()) params = params.set('search', search.trim());
        if (estado) params = params.set('estado', estado);
        if (destino) params = params.set('destino', destino);
        if (fechaInicio) params = params.set('fechaInicio', fechaInicio);
        if (fechaFin) params = params.set('fechaFin', fechaFin);

        return this.http.get<OrdenTrasladoNoFormalizadaPaginado>(this.apiUrl, { params });
    }

    verificar(dto: VerificarOrdenTrasladoNoFormalizada): Observable<OrdenTrasladoNoFormalizada> {
        return this.http.put<OrdenTrasladoNoFormalizada>(`${this.apiUrl}/verificar`, dto);
    }

    confirmar(id: number): Observable<OrdenTrasladoNoFormalizada> {
        return this.http.put<OrdenTrasladoNoFormalizada>(`${this.apiUrl}/${id}/confirmar`, {});
    }
}