import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class VinculacionesFlotaChatService {
  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  constructor(private http: HttpClient) {}

  obtenerPendientes(tipo: string) {
    return this.http.get<any[]>(`${environment.apiUrl}/VinculacionesFlotaChat/usuarios-flotachat-pendientes?tipo=${tipo}`, { headers: this.headers });
  }

  obtenerVinculadas(tipo: string) {
    return this.http.get<any[]>(`${environment.apiUrl}/VinculacionesFlotaChat?tipoEntidad=${tipo}`, { headers: this.headers });
  }

  obtenerEntidades(tipo: string) {
    return this.http.get<any[]>(`${environment.apiUrl}/VinculacionesFlotaChat/entidades?tipo=${tipo}`, { headers: this.headers });
  }

  vincular(flotaChatUsuarioId: number, tipoEntidad: string, entidadId: number, telefono: string) {
    const body = { flotaChatUsuarioId, tipoEntidad, entidadId, telefono };
    return this.http.post(`${environment.apiUrl}/VinculacionesFlotaChat`, body, { headers: this.headers });
  }

  eliminar(id: number) {
    return this.http.delete(`${environment.apiUrl}/VinculacionesFlotaChat/${id}`, { headers: this.headers });
  }
}