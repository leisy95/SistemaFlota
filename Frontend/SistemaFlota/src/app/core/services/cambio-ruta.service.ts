import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CambioRutaService {

  private apiUrl = `${environment.apiUrl}/CambioRuta`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  obtenerPendientes(): Observable<any> {
    return this.http.get(`${this.apiUrl}/pendientes`);
  }

  crear(dto: any): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  autorizar(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/autorizar`, dto);
  }

  rechazar(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/rechazar`, dto);
  }

  // ✅ Conductor confirma que recibió la autorización
  confirmar(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/confirmar`, {});
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}