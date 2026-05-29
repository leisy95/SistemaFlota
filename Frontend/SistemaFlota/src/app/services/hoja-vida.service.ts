import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class HojaVidaService {

  private api = `${environment.apiUrl}/HojaVida`;

  constructor(private http: HttpClient) {}

  obtenerHojaVida(conductorId: number): Observable<any> {
    return this.http.get<any>(`${this.api}/${conductorId}`);
  }

  actualizarConductor(conductorId: number, datos: any): Observable<any> {
    return this.http.put<any>(`${this.api}/${conductorId}`, datos);
  }

  // Exámenes
  agregarExamen(conductorId: number, form: FormData): Observable<any> {
    return this.http.post<any>(`${this.api}/${conductorId}/examenes`, form);
  }
  eliminarExamen(id: number): Observable<any> {
    return this.http.delete(`${this.api}/examenes/${id}`);
  }

  // Capacitaciones
  agregarCapacitacion(conductorId: number, form: FormData): Observable<any> {
    return this.http.post<any>(`${this.api}/${conductorId}/capacitaciones`, form);
  }
  eliminarCapacitacion(id: number): Observable<any> {
    return this.http.delete(`${this.api}/capacitaciones/${id}`);
  }

  // Infracciones
  agregarInfraccion(conductorId: number, form: FormData): Observable<any> {
    return this.http.post<any>(`${this.api}/${conductorId}/infracciones`, form);
  }
  actualizarEstadoInfraccion(id: number, estado: string): Observable<any> {
    return this.http.put(`${this.api}/infracciones/${id}/estado`, { estado });
  }
  eliminarInfraccion(id: number): Observable<any> {
    return this.http.delete(`${this.api}/infracciones/${id}`);
  }
}