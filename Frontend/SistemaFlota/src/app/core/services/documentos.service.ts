import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DocumentosService {

  private apiUrl = `${environment.apiUrl}/Documentos`;

  constructor(private http: HttpClient) {}

  // VEHÍCULO
  obtenerDocumentosVehiculo(vehiculoId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/vehiculo/${vehiculoId}`);
  }

  subirDocumentoVehiculo(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/vehiculo`, formData);
  }

  eliminarDocumentoVehiculo(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/vehiculo/${id}`);
  }

  // GENERALES
  obtenerDocumentosGenerales(categoria?: string): Observable<any[]> {
    const params = categoria ? `?categoria=${categoria}` : '';
    return this.http.get<any[]>(`${this.apiUrl}/generales${params}`);
  }

  subirDocumentoGeneral(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/generales`, formData);
  }

  eliminarDocumentoGeneral(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/generales/${id}`);
  }

  // POR VENCER
  obtenerPorVencer(): Observable<any> {
    return this.http.get(`${this.apiUrl}/por-vencer`);
  }
}