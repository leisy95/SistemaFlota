
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LeerReferenciaResponse } from '../../models/produccion/leer-referencia.model';

export interface LectorReferenciaDto {
    texto: string;
    leidaCorrectamente: boolean;
    mensaje: string;
}

@Injectable({ providedIn: 'root' })
export class LectorReferenciaService {

    private apiUrl = `${environment.apiUrl}/produccion/retal`;

    constructor(private http: HttpClient) { }

    leerReferencia(imagen: File): Observable<LeerReferenciaResponse> {
        const formData = new FormData();
        formData.append('imagen', imagen);

        return this.http.post<LeerReferenciaResponse>(
            `${this.apiUrl}/leer-referencia`,
            formData
        );
    }
}
