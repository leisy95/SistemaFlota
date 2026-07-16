import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vehiculo } from '../models/vehiculo.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class VehiculosService {

  private apiUrl =
    `${environment.apiUrl}/Vehiculos`;

  constructor(private http: HttpClient) {}

  // OBTENER
  obtenerVehiculos(): Observable<Vehiculo[]> {
    return this.http.get<Vehiculo[]>(this.apiUrl);
  }

  // CREAR
  crearVehiculo(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }

  // EDITAR
  editarVehiculo(id: number, formData: FormData): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, formData);
  }

  // ELIMINAR
  eliminarVehiculo(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // TOTAL DASHBOARD
  obtenerTotalVehiculos() {
    return this.http.get<any[]>(this.apiUrl); // ← corregido
  }

}