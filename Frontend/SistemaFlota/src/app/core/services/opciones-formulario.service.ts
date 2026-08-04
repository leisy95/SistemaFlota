import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OpcionesFormularioService {
  private api = `${environment.apiUrl}/OpcionesFormulario`;

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  getOpciones(categoria: string, tipoFormatoId?: number) {
    let url = `${this.api}?categoria=${categoria}`;
    if (tipoFormatoId) url += `&tipoFormatoId=${tipoFormatoId}`;
    return this.http.get<any[]>(url, { headers: this.headers });
  }

  getTodas() {
    return this.http.get<any[]>(`${this.api}/todas`, { headers: this.headers });
  }

  crear(dto: any) {
    return this.http.post<any>(this.api, dto, { headers: this.headers });
  }

  editar(id: number, dto: any) {
    return this.http.put<any>(`${this.api}/${id}`, dto, { headers: this.headers });
  }

  cambiarEstado(id: number) {
    return this.http.put<any>(`${this.api}/${id}/estado`, {}, { headers: this.headers });
  }

  eliminar(id: number) {
    return this.http.delete<void>(`${this.api}/${id}`, { headers: this.headers });
  }

  constructor(private http: HttpClient) {}
}