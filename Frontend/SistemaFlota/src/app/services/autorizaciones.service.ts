import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AutorizacionesService {

  // El interceptor (auth.interceptor.ts) agrega el Bearer token
  // automáticamente a TODAS las peticiones — no hay que hacerlo aquí.
  private apiUrl = `${environment.apiUrl}/Autorizaciones`;

  constructor(private http: HttpClient) {}

  obtenerAutorizaciones(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  crear(datos: any): Observable<any> {
    return this.http.post(this.apiUrl, datos);
  }

  firmarFacturacion(id: number, datos: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/facturacion`, datos);
  }

  firmarBodega(id: number, datos: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/bodega`, datos);
  }

  firmarPorteria(id: number, datos: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/porteria`, datos);
  }

  rechazar(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/rechazar`, {});
  }

  generarGuia(): Observable<any> {
    return this.http.get(`${this.apiUrl}/generar-guia`);
  }
}