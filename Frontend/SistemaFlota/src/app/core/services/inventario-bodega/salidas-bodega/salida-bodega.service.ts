import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SalidaBodegaActual } from '../../../models/inventario-bodega/salidas-bodega/salida-bodega.model';
import { ConfirmarSalidaBodegaDto, RespuestaSalidaBodegaDto } from '../../../models/inventario-bodega/salidas-bodega/salida-bodega.dto';

@Injectable({
    providedIn: 'root'
})
export class SalidaBodegaService {

    private http = inject(HttpClient);

    private readonly apiUrl =
        `${environment.apiUrl}/SalidasBodega`;

    obtenerSalidaActual(): Observable<SalidaBodegaActual> {
        return this.http.get<SalidaBodegaActual>(
            `${this.apiUrl}/actual`
        );
    }

    crearSalida(): Observable<RespuestaSalidaBodegaDto> {
        return this.http.post<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/crear`,
            {}
        );
    }

    // AGREGAR ITEM POR REFERENCIA
    agregarPorReferencia(idSalida: number, referenciaPeso: string): Observable<RespuestaSalidaBodegaDto> {
        return this.http.post<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/${idSalida}/agregar-por-referencia`,
            JSON.stringify(referenciaPeso),
            { headers: { 'Content-Type': 'application/json' } }
        );
    }

    agregarItem(
        idSalida: number,
        idInventarioBodegaItem: number
    ): Observable<RespuestaSalidaBodegaDto> {
        return this.http.post<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/${idSalida}/items/${idInventarioBodegaItem}`,
            {}
        );
    }

    quitarItem(
        idSalida: number,
        idInventarioBodegaItem: number
    ): Observable<RespuestaSalidaBodegaDto> {
        return this.http.delete<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/${idSalida}/items/${idInventarioBodegaItem}`
        );
    }

    confirmarSalida(
        idSalida: number,
        dto: ConfirmarSalidaBodegaDto
    ): Observable<RespuestaSalidaBodegaDto> {
        return this.http.post<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/${idSalida}/confirmar`,
            dto
        );
    }

    cancelarSalida(
        idSalida: number
    ): Observable<RespuestaSalidaBodegaDto> {
        return this.http.post<RespuestaSalidaBodegaDto>(
            `${this.apiUrl}/${idSalida}/cancelar`,
            {}
        );
    }
}