import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ContactosService {

  private apiUrl = `${environment.apiUrl}/Contactos`;

  constructor(private http: HttpClient) {}

  // OBTENER TODOS
  obtenerContactos(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  // CREAR
  crearContacto(contacto: any): Observable<any> {
    return this.http.post(this.apiUrl, contacto);
  }

  // EDITAR
  editarContacto(id: number, contacto: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, contacto);
  }

  // ELIMINAR
  eliminarContacto(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // CAMBIAR ESTADO
  cambiarEstado(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/estado`, {});
  }

}