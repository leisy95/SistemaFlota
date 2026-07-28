import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class OrdenesProduccionService {
  private api = `${environment.apiUrl}/OrdenesProduccion`;

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  buscar(numeroOP: string) {
    return this.http.get<any>(`${this.api}/buscar/${numeroOP}`, { headers: this.headers });
  }

  importar(archivo: File) {
    const fd = new FormData();
    fd.append('archivo', archivo, archivo.name);
    return this.http.post<any>(`${this.api}/importar`, fd, { headers: this.headers });
  }

  constructor(private http: HttpClient) {}
}