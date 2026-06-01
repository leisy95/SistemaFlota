import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UsuariosService {

  private apiUrl = `${environment.apiUrl}/Usuarios`;

  constructor(private http: HttpClient) {}

  // GET TODOS — con paginación y búsqueda
  obtenerUsuarios(
    pagina    = 1,
    porPagina = 20,
    buscar    = ''
  ): Observable<any> {
    let params = new HttpParams()
      .set('pagina',    pagina.toString())
      .set('porPagina', porPagina.toString());

    if (buscar?.trim())
      params = params.set('buscar', buscar.trim());

    return this.http.get<any>(this.apiUrl, { params });
  }

  // GET POR ID
  obtenerPorId(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  // POST
  crearUsuario(usuario: any): Observable<any> {
    return this.http.post(this.apiUrl, usuario);
  }

  // PUT
  actualizarUsuario(id: number, usuario: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, usuario);
  }

  // DELETE
  eliminarUsuario(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // CAMBIAR ESTADO
  cambiarEstado(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/estado`, {});
  }

  // RECUPERAR CONTRASEÑA
  solicitarRecuperacion(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/recuperar`, { email });
  }

  // CAMBIAR CONTRASEÑA CON TOKEN
  cambiarPassword(email: string, token: string, nuevaPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/cambiar-password`, { email, token, nuevaPassword });
  }

  // MIS PERMISOS
  misPermisos(): Observable<any> {
    return this.http.get(`${this.apiUrl}/mis-permisos`);
  }
}