
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';

import { ClienteBodega, ClienteBodegaFiltro, ClientesBodegaPaginados } from '../../../models/inventario-bodega/clientes/cliente-bodega.model';

import {
    ClienteBodegaRequestDto,
    EditarClienteBodegaDto,
    RespuestaClienteBodegaDto
} from '../../../models/inventario-bodega/clientes/cliente-bodega.dto';

@Injectable({
    providedIn: 'root'
})
export class ClienteBodegaService {

    private http = inject(HttpClient);

    private readonly apiUrl = `${environment.apiUrl}/ClientesBodega`;

    // LISTAR CLIENTES
    listarClientes(
        filtros: ClienteBodegaFiltro
    ) {
        let params = new HttpParams()
            .set('pagina', filtros.pagina)
            .set('pageSize', filtros.pageSize);

        if (filtros.buscar.trim()) {
            params = params.set('buscar', filtros.buscar.trim());
        }

        if (filtros.nombre.trim()) {
            params = params.set('nombre', filtros.nombre.trim());
        }

        if (filtros.telefono.trim()) {
            params = params.set('telefono', filtros.telefono.trim());
        }

        if (filtros.email.trim()) {
            params = params.set('email', filtros.email.trim());
        }

        return this.http.get<ClientesBodegaPaginados>(
            `${this.apiUrl}/listar`,
            { params }
        );
    }

    // OBTENER CLIENTE POR ID
    obtenerCliente(id: number): Observable<ClienteBodega> {
        return this.http.get<ClienteBodega>(
            `${this.apiUrl}/${id}`
        );
    }

    // CREAR CLIENTE
    crearCliente(
        dto: ClienteBodegaRequestDto
    ): Observable<RespuestaClienteBodegaDto> {
        return this.http.post<RespuestaClienteBodegaDto>(
            `${this.apiUrl}/crear`,
            dto
        );
    }

    // ACTUALIZAR CLIENTE
    actualizarCliente(
        dto: EditarClienteBodegaDto
    ): Observable<RespuestaClienteBodegaDto> {
        return this.http.put<RespuestaClienteBodegaDto>(
            `${this.apiUrl}/actualizar/${dto.id}`,
            dto
        );
    }

    // ELIMINAR CLIENTE
    eliminarCliente(id: number): Observable<RespuestaClienteBodegaDto> {
        return this.http.delete<RespuestaClienteBodegaDto>(
            `${this.apiUrl}/eliminar/${id}`
        );
    }
}
