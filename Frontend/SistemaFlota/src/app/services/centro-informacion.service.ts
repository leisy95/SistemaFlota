import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CentroInformacionService {
  private apiUrl = `${environment.apiUrl}/CentroInformacion`;
  constructor(private http: HttpClient) {}

  getRutas(): Observable<any[]> { return this.http.get<any[]>(`${this.apiUrl}/rutas`); }
  crearRuta(fd: FormData): Observable<any> { return this.http.post(`${this.apiUrl}/rutas`, fd); }
  eliminarRuta(id: number): Observable<any> { return this.http.delete(`${this.apiUrl}/rutas/${id}`); }

  getEmergencia(): Observable<any[]> { return this.http.get<any[]>(`${this.apiUrl}/emergencia`); }
  crearEmergencia(dto: any): Observable<any> { return this.http.post(`${this.apiUrl}/emergencia`, dto); }
  actualizarEmergencia(id: number, dto: any): Observable<any> { return this.http.put(`${this.apiUrl}/emergencia/${id}`, dto); }
  eliminarEmergencia(id: number): Observable<any> { return this.http.delete(`${this.apiUrl}/emergencia/${id}`); }
}
