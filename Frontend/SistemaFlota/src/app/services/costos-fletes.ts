import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CostosFleteService {
  private apiUrl = `${environment.apiUrl}/CostosFletes`;

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  constructor(private http: HttpClient) {}

  getRegistros(filtros?: any): Observable<any[]> {
    let url = this.apiUrl + '?';
    if (filtros?.desde) url += `desde=${filtros.desde}&`;
    if (filtros?.hasta) url += `hasta=${filtros.hasta}&`;
    if (filtros?.conductor) url += `conductor=${filtros.conductor}&`;
    if (filtros?.estado) url += `estado=${filtros.estado}&`;
    if (filtros?.ciudad) url += `ciudad=${filtros.ciudad}&`;
    return this.http.get<any[]>(url, { headers: this.headers });
  }

  crear(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, { headers: this.headers });
  }

  editar(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data, { headers: this.headers });
  }

  verificar(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/verificar`, data, { headers: this.headers });
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, { headers: this.headers });
  }
}