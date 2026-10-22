import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CostosFleteService {
  private apiUrl = `${environment.apiUrl}/CostosFletes`;

  private get headers(): HttpHeaders {
    return new HttpHeaders({
      Authorization: 'Bearer ' + sessionStorage.getItem('token')
    });
  }

  constructor(private http: HttpClient) { }

  getRegistros(filtros?: any): Observable<any[]> {
    let params = new HttpParams();

    if (filtros?.desde) {
      params = params.set('desde', filtros.desde);
    }

    if (filtros?.hasta) {
      params = params.set('hasta', filtros.hasta);
    }

    if (filtros?.conductor?.trim()) {
      params = params.set('conductor', filtros.conductor.trim());
    }

    if (filtros?.estado) {
      params = params.set('estado', filtros.estado);
    }

    if (filtros?.ciudad?.trim()) {
      params = params.set('ciudad', filtros.ciudad.trim());
    }

    if (filtros?.placa?.trim()) {
      params = params.set('placa', filtros.placa.trim());
    }

    return this.http.get<any[]>(this.apiUrl, {
      headers: this.headers,
      params
    });
  }

  // Generar y descargar el PDF desde el backend
  generarPdf(filtros?: any): Observable<Blob> {
    let params = new HttpParams();

    if (filtros?.desde) {
      params = params.set('desde', filtros.desde);
    }

    if (filtros?.hasta) {
      params = params.set('hasta', filtros.hasta);
    }

    if (filtros?.conductor?.trim()) {
      params = params.set('conductor', filtros.conductor.trim());
    }

    if (filtros?.estado) {
      params = params.set('estado', filtros.estado);
    }

    if (filtros?.ciudad?.trim()) {
      params = params.set('ciudad', filtros.ciudad.trim());
    }

    if (filtros?.placa?.trim()) {
      params = params.set('placa', filtros.placa.trim());
    }

    return this.http.get(`${this.apiUrl}/pdf`, {
      headers: this.headers,
      params,
      responseType: 'blob'
    });
  }

  crear(data: any): Observable<any> {
    return this.http.post(this.apiUrl, data, {
      headers: this.headers
    });
  }

  editar(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data, {
      headers: this.headers
    });
  }

  verificar(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/verificar`, data, {
      headers: this.headers
    });
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.headers
    });
  }
}