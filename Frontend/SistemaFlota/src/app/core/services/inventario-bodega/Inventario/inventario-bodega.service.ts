
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
    ActualizarInventarioBodegaDto,
    InventarioBodega,
    RecalcularInventarioBodegaResponseDto,
    RegistrarItemDto,
    RegistrarItemResponseDto
} from '../../../models/inventario-bodega/inventario-bodega.models';

@Injectable({
    providedIn: 'root'
})
export class InventarioBodegaService {

    private readonly apiUrl = `${environment.apiUrl}/InventarioBodega`;

    constructor(private http: HttpClient) { }

    // OBTENER TODOS
    obtener(): Observable<InventarioBodega[]> {
        return this.http.get<InventarioBodega[]>(this.apiUrl);
    }

    // OBTENER PAGINADO
    obtenerPaginado(
        buscar: string,
        material: string,
        color: string,
        pagina: number,
        pageSize: number
    ) {
        let params = new HttpParams()
            .set('pagina', pagina)
            .set('pageSize', pageSize);

        if (buscar.trim()) {
            params = params.set('buscar', buscar.trim());
        }

        if (material) {
            params = params.set('material', material);
        }

        if (color) {
            params = params.set('color', color);
        }

        return this.http.get<any>(
            `${this.apiUrl}/paginado`,
            { params }
        );
    }

    // BUSCAR POR REFERENCIA
    obtenerPorReferencia(
        referencia: string
    ): Observable<InventarioBodega> {
        return this.http.get<InventarioBodega>(
            `${this.apiUrl}/referencia/${encodeURIComponent(referencia)}`
        );
    }

    // REGISTRAR PRODUCTO
    registrarItem(
        request: RegistrarItemDto
    ): Observable<RegistrarItemResponseDto> {
        return this.http.post<RegistrarItemResponseDto>(
            `${this.apiUrl}/registrar`,
            request
        );
    }

    // EDITAR INVENTARIO
    editar(
        id: number,
        request: ActualizarInventarioBodegaDto
    ): Observable<{ message: string }> {
        return this.http.put<{ message: string }>(
            `${this.apiUrl}/editar/${id}`,
            request
        );
    }

    // RECALCULAR INVENTARIO
    recalcular(): Observable<RecalcularInventarioBodegaResponseDto> {
        return this.http.post<RecalcularInventarioBodegaResponseDto>(
            `${this.apiUrl}/recalcular`,
            {}
        );
    }

    // ELIMINAR INVENTARIO
    eliminar(id: number): Observable<{ message: string }> {
        return this.http.delete<{ message: string }>(
            `${this.apiUrl}/eliminar/${id}`
        );
    }
}
