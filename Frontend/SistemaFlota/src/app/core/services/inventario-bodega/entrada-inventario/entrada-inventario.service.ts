
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { IngresarPesoDto, IngresarPesoResponseDto } from '../../../models/inventario-bodega/inventario-bodega.models';
import { DevolucionEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/devoluciones.models';
import { AjusteGeneralEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/inventario-bodega.models';
import { EntradasBodegaPaginadasDto, MostrarEntradaDto } from '../../../models/inventario-bodega/entrada-bodega/entradas-bodega.models';

@Injectable({
    providedIn: 'root'
})
export class EntradaInventarioService {

    private readonly apiUrl = `${environment.apiUrl}/EntradaInventario`;

    constructor(private http: HttpClient) { }

    // OBTENER ENTRADAS DE INVENTARIO
    obtenerEntradas(estado?: string): Observable<MostrarEntradaDto[]> {

        let params = new HttpParams();

        if (estado) {
            params = params.set('estado', estado);
        }

        return this.http.get<MostrarEntradaDto[]>(
            `${this.apiUrl}/mostrar-entradas`,
            { params: params }
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

    filtrarEntradas(
        buscar?: string,
        estado?: string,
        fechaInicio?: string,
        fechaFin?: string,
        pagina: number = 1,
        pageSize: number = 5
    ): Observable<EntradasBodegaPaginadasDto> {

        let params = new HttpParams()
            .set('pagina', pagina)
            .set('pageSize', pageSize);

        if (buscar?.trim()) {
            params = params.set('buscar', buscar.trim());
        }

        if (estado?.trim()) {
            params = params.set('estado', estado.trim());
        }

        if (fechaInicio) {
            params = params.set('fechaInicio', fechaInicio);
        }

        if (fechaFin) {
            params = params.set('fechaFin', fechaFin);
        }

        return this.http.get<EntradasBodegaPaginadasDto>(
            `${this.apiUrl}/filtrar-entradas`,
            { params }
        );
    }

    // EXPORTAR ENTRADAS DE INVENTARIO A PDF
    exportarPdf(
        buscar?: string,
        fechaInicio?: string,
        fechaFin?: string
    ): Observable<Blob> {

        let params = new HttpParams();

        if (buscar?.trim()) {
            params = params.set('buscar', buscar.trim());
        }

        if (fechaInicio) {
            params = params.set('fechaInicio', fechaInicio);
        }

        if (fechaFin) {
            params = params.set('fechaFin', fechaFin);
        }

        return this.http.get(
            `${this.apiUrl}/exportar-pdf`,
            {
                params,
                responseType: 'blob'
            }
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
