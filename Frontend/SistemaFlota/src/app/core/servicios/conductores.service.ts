import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Conductor {
  id?: number;
  nombre: string;
  licencia: string;
  telefono: string;
  email: string;
  foto?: string; // ðŸ”¥ NUEVO
}

@Injectable({ providedIn: 'root' })
export class ConductoresService {

  private apiUrl = 'https://api.gecobagsci.com/api/Conductores';

  constructor(private http: HttpClient) {}

  obtenerConductores(): Observable<Conductor[]> {
    return this.http.get<Conductor[]>(this.apiUrl);
  }

  // ðŸ”¥ AHORA RECIBE FORM DATA
  crearConductor(data: FormData) {
    return this.http.post(this.apiUrl, data);
  }

  actualizarConductor(conductor: Conductor) {
    return this.http.put(`${this.apiUrl}/${conductor.id}`, conductor);
  }

  eliminarConductor(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
