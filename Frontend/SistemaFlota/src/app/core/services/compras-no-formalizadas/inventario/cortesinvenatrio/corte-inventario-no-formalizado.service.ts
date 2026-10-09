import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { InventarioCorteNoFormalizado } from '../../../../models/compras-no-formalizadas/inventario/cortes-inventario/corte-inventario-no-formalizado.model';
import { CrearCorteInventarioNoFormalizado } from '../../../../models/compras-no-formalizadas/inventario/cortes-inventario/crear-corte-inventario-no-formalizado.model';
import { CorteInventarioHistorialNoFormalizado } from '../../../../models/compras-no-formalizadas/inventario/historial-corte-inventario/corte-inventario-historial-no-formalizado.model';
import { HistorialCorteDetalleNoFormalizado } from '../../../../models/compras-no-formalizadas/inventario/historial-corte-inventario/historial-corte-detalle-no-formalizado.model';

@Injectable({
    providedIn: 'root'
})
export class CorteInventarioNoFormalizadoService {

    private apiUrl = `${environment.apiUrl}/CorteInventarioNoFormalizado`;

    constructor(private http: HttpClient) { }

    obtenerCorte(): Observable<InventarioCorteNoFormalizado[]> {
        return this.http.get<InventarioCorteNoFormalizado[]>(
            this.apiUrl
        );
    }

    guardarCorte(
        dto: CrearCorteInventarioNoFormalizado
    ): Observable<any> {
        return this.http.post<any>(
            this.apiUrl,
            dto
        );
    }

    obtenerHistorial(): Observable<CorteInventarioHistorialNoFormalizado[]> {
        return this.http.get<CorteInventarioHistorialNoFormalizado[]>(
            `${this.apiUrl}/historial`
        );
    }

    obtenerDetalle(
        id: number
    ): Observable<HistorialCorteDetalleNoFormalizado> {
        return this.http.get<HistorialCorteDetalleNoFormalizado>(
            `${this.apiUrl}/${id}`
        );
    }


    generarPdf(
        material: string = '',
        proveedor: string = ''
    ): Observable<Blob> {

        const params: any = {};

        if (material.trim()) {
            params.material = material.trim();
        }

        if (proveedor.trim()) {
            params.proveedor = proveedor.trim();
        }

        return this.http.get(
            `${this.apiUrl}/pdf`,
            {
                params,
                responseType: 'blob'
            }
        );
    }
}