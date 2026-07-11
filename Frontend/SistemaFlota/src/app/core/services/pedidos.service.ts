import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PedidosService {

  private apiUrl = `${environment.apiUrl}/Pedidos`;

  constructor(private http: HttpClient) {}

  obtenerTodos(params?: {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
    estado?: string;
    prioridad?: string;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.pagina)    httpParams = httpParams.set('pagina',    params.pagina.toString());
    if (params?.porPagina) httpParams = httpParams.set('porPagina', params.porPagina.toString());
    if (params?.buscar)    httpParams = httpParams.set('buscar',    params.buscar);
    if (params?.estado)    httpParams = httpParams.set('estado',    params.estado);
    if (params?.prioridad) httpParams = httpParams.set('prioridad', params.prioridad);
    return this.http.get(this.apiUrl, { params: httpParams });
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  crear(dto: any): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  editar(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, dto);
  }

  cambiarEstado(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/estado`, dto);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}