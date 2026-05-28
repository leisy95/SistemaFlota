import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConductoresService {

  private apiUrl =
    `${environment.apiUrl}/Conductores`;

  constructor(private http: HttpClient) {}

  // =========================
  // OBTENER TODOS
  // =========================
  obtenerConductores(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // =========================
  // CREAR
  // =========================
  crearConductor(formData: FormData) {
    return this.http.post(this.apiUrl, formData);
  }

  // =========================
  // EDITAR
  // =========================
  editarConductor(id: number, formData: FormData) {
    return this.http.put(`${this.apiUrl}/${id}`, formData);
  }

  // =========================
  // ELIMINAR
  // =========================
  eliminarConductor(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // =========================
  // TOTAL DASHBOARD
  // =========================
  obtenerTotalConductores() {
    return this.http.get<any[]>(this.apiUrl); // ← corregido
  }

}