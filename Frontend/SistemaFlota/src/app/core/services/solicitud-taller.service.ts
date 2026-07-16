import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SolicitudTallerService {

  private apiUrl = `${environment.apiUrl}/SolicitudTaller`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  crear(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  autorizar(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/autorizar`, dto);
  }

  rechazar(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/rechazar`, dto);
  }

  marcarEnTaller(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/en-taller`, {});
  }

  // ✅ Conductor confirma que recibió la autorización
  confirmar(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/confirmar`, {});
  }

  registrarFactura(id: number, dto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/factura`, dto);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}