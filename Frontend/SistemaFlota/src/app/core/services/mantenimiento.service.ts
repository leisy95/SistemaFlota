import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MantenimientoService {

  private apiUrl = `${environment.apiUrl}/Mantenimiento`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  obtenerPorVehiculo(vehiculoId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/vehiculo/${vehiculoId}`);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  crear(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  editar(id: number, formData: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, formData);
  }

  finalizar(id: number, datos: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/finalizar`, datos);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  obtenerProximos(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/proximos`);
  }
}