import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CyrelesService {
  private api = `${environment.apiUrl}/Cyreles`;

  constructor(private http: HttpClient) {}

  // Cajones
  getCajones(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/cajones`);
  }
  crearCajon(dto: { numero: number; descripcion?: string }): Observable<any> {
    return this.http.post<any>(`${this.api}/cajones`, dto);
  }
  editarCajon(id: number, dto: { numero: number; descripcion?: string }): Observable<any> {
    return this.http.put<any>(`${this.api}/cajones/${id}`, dto);
  }
  eliminarCajon(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/cajones/${id}`);
  }

  // Registros
  getRegistros(filtros?: { cajonId?: number; nombre?: string; cajonNumero?: number }): Observable<any[]> {
    let params = new HttpParams();
    if (filtros?.cajonId)     params = params.set('cajonId',     filtros.cajonId.toString());
    if (filtros?.nombre)      params = params.set('nombre',      filtros.nombre);
    if (filtros?.cajonNumero) params = params.set('cajonNumero', filtros.cajonNumero.toString());
    return this.http.get<any[]>(`${this.api}/registros`, { params });
  }
  getRegistro(id: number): Observable<any> {
    return this.http.get<any>(`${this.api}/registros/${id}`);
  }
  crearRegistro(cajonId: number, nombre: string, foto?: File): Observable<any> {
    const fd = new FormData();
    fd.append('cajonId', cajonId.toString());
    fd.append('nombre', nombre);
    if (foto) fd.append('foto', foto, foto.name);
    return this.http.post<any>(`${this.api}/registros`, fd);
  }
  editarRegistro(id: number, cajonId: number, nombre: string, foto?: File): Observable<any> {
    const fd = new FormData();
    fd.append('cajonId', cajonId.toString());
    fd.append('nombre', nombre);
    if (foto) fd.append('foto', foto, foto.name);
    return this.http.put<any>(`${this.api}/registros/${id}`, fd);
  }
  eliminarRegistro(id: number): Observable<void> {
    return this.http.delete<void>(`${this.api}/registros/${id}`);
  }

  getFotoUrl(nombre: string): string {
    return `${environment.fotosUrl}/cyreles/${nombre}`;
  }
}