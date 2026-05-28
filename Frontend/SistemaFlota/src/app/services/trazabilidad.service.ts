import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TrazabilidadService {

  private apiUrl = `${environment.apiUrl}/Trazabilidad`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<any> {
    return this.http.get(this.apiUrl);
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

  // NOTAS
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

  // AUTORIZACIONES DISPONIBLES PARA IMPORTAR
  obtenerAutorizacionesDisponibles(): Observable<any> {
    return this.http.get(`${this.apiUrl}/autorizaciones-disponibles`);
  }
}