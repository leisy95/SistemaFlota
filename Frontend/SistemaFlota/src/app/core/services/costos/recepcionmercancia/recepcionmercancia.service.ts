import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';


@Injectable({
    providedIn: 'root'
})
export class RecepcionMercanciaService {

    private apiUrl = `${environment.apiUrl}/RecepcionMercancia`;

    constructor(
        private http: HttpClient
    ) { }

    obtenerFormulario(
        ordenCompraId: number
    ): Observable<any> {

        return this.http.get<any>(
            `${this.apiUrl}/formulario/${ordenCompraId}`
        );
    }

    crear(data: any): Observable<any> {

        return this.http.post<any>(
            this.apiUrl,
            data
        );
    }

    obtenerEtiquetas(id: number): Observable<Blob> {
        return this.http.get(
            `${this.apiUrl}/${id}/etiquetas`,
            {
                responseType: 'blob'
            }
        );
    }
}