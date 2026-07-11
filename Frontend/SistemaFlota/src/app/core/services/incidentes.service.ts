import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class IncidentesService {

  private apiUrl = `${environment.apiUrl}/Incidentes`;

  constructor(private http: HttpClient) {}

  // OBTENER TODOS
  obtenerIncidentes(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // OBTENER POR ID
  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  // CREAR
  crearIncidente(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  // MARCAR REVISADO
  marcarRevisado(id: number, datos: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/revisar`, datos);
  }

  // CONTACTOS WHATSAPP
  obtenerContactosWhatsApp(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/contactos-whatsapp`);
  }

}