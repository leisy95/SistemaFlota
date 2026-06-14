import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TrazabilidadService {

  private apiUrl = `${environment.apiUrl}/Trazabilidad`;

  constructor(private http: HttpClient) {}

  obtenerTodos(params?: {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
    estado?: string;
    entregada?: string;
    tipo?: string;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.pagina)    httpParams = httpParams.set('pagina',    params.pagina.toString());
    if (params?.porPagina) httpParams = httpParams.set('porPagina', params.porPagina.toString());
    if (params?.buscar)    httpParams = httpParams.set('buscar',    params.buscar);
    if (params?.estado)    httpParams = httpParams.set('estado',    params.estado);
    if (params?.entregada) httpParams = httpParams.set('entregada', params.entregada);
    if (params?.tipo)      httpParams = httpParams.set('tipo',      params.tipo);
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

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  obtenerNotas(trazabilidadId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${trazabilidadId}/notas`);
  }

  agregarNota(trazabilidadId: number, dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${trazabilidadId}/notas`, dto);
  }

  editarNota(notaId: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/notas/${notaId}`, dto);
  }

  eliminarNota(notaId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/notas/${notaId}`);
  }

  obtenerAutorizacionesDisponibles(): Observable<any> {
    return this.http.get(`${this.apiUrl}/autorizaciones-disponibles`);
  }
}