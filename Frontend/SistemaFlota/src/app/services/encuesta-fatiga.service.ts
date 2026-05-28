import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EncuestaFatigaService {

  private apiUrl = `${environment.apiUrl}/EncuestaFatiga`;

  constructor(private http: HttpClient) {}

  obtenerEncuestas(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  obtenerPorConductor(conductorId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/conductor/${conductorId}`);
  }

  obtenerEstadisticas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estadisticas`);
  }

  crear(dto: any): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}