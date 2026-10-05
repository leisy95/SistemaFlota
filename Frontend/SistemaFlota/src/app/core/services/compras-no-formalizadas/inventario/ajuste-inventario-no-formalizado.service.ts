import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Observable } from 'rxjs';
import { InventarioAjusteNoFormalizado } from '../../../models/compras-no-formalizadas/inventario/AjusteInventario/inventario-ajuste-no-formalizado.model';
import { CrearAjusteInventarioNoFormalizado } from '../../../models/compras-no-formalizadas/inventario/AjusteInventario/crear-ajuste-inventario-no-formalizado.model';
import { AjusteInventarioNoFormalizado } from '../../../models/compras-no-formalizadas/inventario/AjusteInventario/ajuste-inventario-no-formalizado.model';

@Injectable({
    providedIn: 'root'
})
export class AjusteInventarioNoFormalizadoService {

    private apiUrl = `${environment.apiUrl}/AjusteInventarioNoFormalizado`;

    constructor(private http: HttpClient) { }

    obtenerInventario(id: number): Observable<InventarioAjusteNoFormalizado> {
        return this.http.get<InventarioAjusteNoFormalizado>(
            `${this.apiUrl}/${id}`
        );
    }

    crear(
        dto: CrearAjusteInventarioNoFormalizado
    ): Observable<AjusteInventarioNoFormalizado> {
        return this.http.post<AjusteInventarioNoFormalizado>(
            this.apiUrl,
            dto
        );
    }

    obtenerHistorial(
        id: number
    ): Observable<AjusteInventarioNoFormalizado[]> {
        return this.http.get<AjusteInventarioNoFormalizado[]>(
            `${this.apiUrl}/historial/${id}`
        );
    }
}