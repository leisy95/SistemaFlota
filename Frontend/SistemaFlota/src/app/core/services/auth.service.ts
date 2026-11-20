
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, { username, password });
  }

  logout(username: string, rol?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/logout`, { username, rol });
  }

  guardarSesion(datos: any): void {
    sessionStorage.setItem('user', JSON.stringify(datos));
    sessionStorage.setItem('token', datos.token ?? '');

    if (datos.tokenBodega) {
      sessionStorage.setItem('tokenBodega', datos.tokenBodega);
    } else {
      sessionStorage.removeItem('tokenBodega');
    }
  }

  obtenerUsuarioActual(): any | null {
    const usuario = sessionStorage.getItem('user');

    return usuario ? JSON.parse(usuario) : null;
  }

  limpiarSesion(): void {
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('tokenBodega');

    localStorage.removeItem('user');
    localStorage.removeItem('token');
    localStorage.removeItem('tokenBodega');
    localStorage.removeItem('rol');
  }

  get username(): string {
    return this.obtenerUsuarioActual()?.username ?? '';
  }

  get rol(): string {
    return this.obtenerUsuarioActual()?.rol ?? '';
  }

  get token(): string {
    return sessionStorage.getItem('token')
      || localStorage.getItem('token')
      || '';
  }

  get tokenBodega(): string {
    return sessionStorage.getItem('tokenBodega') || '';
  }

  get autenticado(): boolean {
    return !!this.token;
  }
}
