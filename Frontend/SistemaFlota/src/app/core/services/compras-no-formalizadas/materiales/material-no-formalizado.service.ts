import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MaterialNoFormalizado } from '../../../models/compras-no-formalizadas/materiales/material-no-formalizado.model';
import { Color } from '../../../models/costos/colores/color.model';
import { Categoria } from '../../../models/costos/categorias/categoria.models';
import { environment } from '../../../../../environments/environment';

export interface MaterialNoFormalizadoPaginado {
    datos: MaterialNoFormalizado[];
    totalRegistros: number;
    pagina: number;
    tamanoPagina: number;
    totalPaginas: number;
}

export interface FiltrosMaterialNoFormalizado {
    proveedores: ProveedorNoFormalizadoFiltro[];
    colores: string[];
}

export interface ProveedorNoFormalizadoFiltro {
    idProveedorNoFormalizado: number;
    nombre: string;
}

@Injectable({
    providedIn: 'root'
})
export class MaterialNoFormalizadoService {

    private apiUrl = `${environment.apiUrl}/MaterialesNoFormalizados`;

    constructor(private http: HttpClient) { }

    obtener(
        search: string = '',
        estado: string = '',
        orden: string = 'nombre',
        proveedor: string = '',
        color: string = '',
        pagina: number = 1,
        tamanoPagina: number = 10
    ): Observable<MaterialNoFormalizadoPaginado> {

        let params = new HttpParams()
            .set('search', search)
            .set('estado', estado)
            .set('orden', orden)
            .set('page', pagina)
            .set('pageSize', tamanoPagina);

        if (proveedor)
            params = params.set('proveedor', proveedor);

        if (color)
            params = params.set('color', color);

        return this.http.get<MaterialNoFormalizadoPaginado>(
            this.apiUrl,
            { params }
        );
    }

    obtenerPorId(id: number): Observable<MaterialNoFormalizado> {
        return this.http.get<MaterialNoFormalizado>(
            `${this.apiUrl}/${id}`
        );
    }

    obtenerColores(): Observable<Color[]> {
        return this.http.get<Color[]>(
            `${this.apiUrl}/colores`
        );
    }

    obtenerCategorias(): Observable<Categoria[]> {
        return this.http.get<Categoria[]>(
            `${this.apiUrl}/categorias`
        );
    }

    obtenerFiltros(): Observable<FiltrosMaterialNoFormalizado> {
        return this.http.get<FiltrosMaterialNoFormalizado>(
            `${this.apiUrl}/filtros`
        );
    }

    crear(formData: FormData): Observable<MaterialNoFormalizado> {
        return this.http.post<MaterialNoFormalizado>(
            this.apiUrl,
            formData
        );
    }

    actualizar(
        id: number,
        formData: FormData
    ): Observable<void> {
        return this.http.put<void>(
            `${this.apiUrl}/${id}`,
            formData
        );
    }
}