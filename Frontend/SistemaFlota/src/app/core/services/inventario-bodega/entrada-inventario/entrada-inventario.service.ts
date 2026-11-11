
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
    IngresarPesoDto,
    IngresarPesoResponseDto
} from '../../../models/inventario-bodega/inventario-bodega.models';

@Injectable({
    providedIn: 'root'
})
export class EntradaInventarioService {

    private readonly apiUrl = `${environment.apiUrl}/EntradaInventario`;

    constructor(private http: HttpClient) { }

    // REGISTRAR ENTRADA DE INVENTARIO
    ingresarPeso(
        request: IngresarPesoDto
    ): Observable<IngresarPesoResponseDto> {
        return this.http.post<IngresarPesoResponseDto>(
            `${this.apiUrl}/ingresar-peso`,
            request
        );
    }
}
