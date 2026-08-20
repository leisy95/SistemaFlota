import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';
import { Observable } from 'rxjs';
import { InventarioAjuste } from '../../../models/costos/inventario/ajusteinventario/inventarioajuste.models';
import { AjusteInventario } from '../../../models/costos/inventario/ajusteinventario/ajusteinventario.models';
import { CrearAjusteInventario } from '../../../models/costos/inventario/ajusteinventario/crearajusteinventario.models';

@Injectable({
    providedIn: 'root'
})
export class AjusteInventarioService {

    private apiUrl = `${environment.apiUrl}/AjusteInventario`;

    constructor(private http: HttpClient) { }

    obtenerInventario(id: number): Observable<InventarioAjuste> {
        return this.http.get<InventarioAjuste>(`${this.apiUrl}/${id}`);
    }

    crear(dto: CrearAjusteInventario): Observable<AjusteInventario> {
        return this.http.post<AjusteInventario>(this.apiUrl, dto);
    }

    obtenerHistorial(id: number): Observable<AjusteInventario[]> {
        return this.http.get<AjusteInventario[]>(`${this.apiUrl}/historial/${id}`);
    }
}