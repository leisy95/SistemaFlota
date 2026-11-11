
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { IngresarPesoDto, IngresarPesoResponseDto } from '../../../models/inventario-bodega/inventario-bodega.models';
import { DevolucionEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/devoluciones.models';
import { AjusteGeneralEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/inventario-bodega.models';
import { MostrarEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/entradas-bodega.models';

@Injectable({
    providedIn: 'root'
})
export class EntradaInventarioService {

    private readonly apiUrl = `${environment.apiUrl}/EntradaInventario`;

    constructor(private http: HttpClient) { }

    // OBTENER ENTRADAS DE INVENTARIO
    obtenerEntradas(): Observable<MostrarEntradaDto[]> {
        return this.http.get<MostrarEntradaDto[]>(
            `${this.apiUrl}/mostrar-entradas`
        );
    }

    // REGISTRAR ENTRADA DE INVENTARIO
    ingresarPeso(
        request: IngresarPesoDto
    ): Observable<IngresarPesoResponseDto> {
        return this.http.post<IngresarPesoResponseDto>(
            `${this.apiUrl}/ingresar-peso`,
            request
        );
    }

    // REGISTRAR DEVOLUCIÓN
    registrarDevolucion(
        request: DevolucionEntradaDto
    ): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(
            `${this.apiUrl}/registrar-devolucion`,
            request
        );
    }

    // AJUSTE GENERAL DE INVENTARIO
    ajusteGeneral(
        request: AjusteGeneralEntradaDto
    ): Observable<{ message: string }> {
        return this.http.post<{ message: string }>(
            `${this.apiUrl}/ajuste-general`,
            request
        );
    }
}
