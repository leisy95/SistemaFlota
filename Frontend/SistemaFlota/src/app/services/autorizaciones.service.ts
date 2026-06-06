import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AutorizacionesService {

  private api = `${environment.apiUrl}/Autorizaciones`;

  constructor(private http: HttpClient) {}

  obtenerAutorizaciones(): Observable<any[]> {
    return this.http.get<any[]>(this.api);
  }

  crear(datos: any): Observable<any> {
    return this.http.post<any>(this.api, datos);
  }

  // ✅ Editar autorización
  editar(id: number, datos: any): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}`, datos);
  }

  generarGuia(): Observable<any> {
    return this.http.get<any>(`${this.api}/generar-guia`);
  }

  firmarFacturacion(id: number, datos: any): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/facturacion`, datos);
  }

  firmarBodega(id: number, datos: any): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/bodega`, datos);
  }

  firmarPorteria(id: number, datos: any): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/porteria`, datos);
  }

  // ── Llegada ───────────────────────────────────────────────────────────────
  reportarLlegada(id: number, datos: { kilometrajeFinal: number | null; novedadesViaje: string; estadoVehiculo: string }): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/reportar-llegada`, datos);
  }

  confirmarLlegada(id: number, datos: { firma: string; usuario: string; observacion: string }): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/confirmar-llegada`, datos);
  }

  // ── Salida en Ruta ────────────────────────────────────────────────────────
  confirmarSalida(id: number): Observable<any> {
    return this.http.put<any>(`${this.api}/${id}/confirmar-salida`, {});
  }
}