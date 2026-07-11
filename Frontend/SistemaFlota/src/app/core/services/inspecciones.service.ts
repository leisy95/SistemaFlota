import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InspeccionesService {

  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  guardarInspeccion(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/Inspecciones`, formData);
  }

  obtenerHistorial(params?: {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.pagina) httpParams = httpParams.set('pagina', params.pagina.toString());
    if (params?.porPagina) httpParams = httpParams.set('porPagina', params.porPagina.toString());
    if (params?.buscar) httpParams = httpParams.set('buscar', params.buscar);
    return this.http.get(`${this.apiUrl}/Inspecciones`, { params: httpParams });
  }

  obtenerDetalle(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/Inspecciones/${id}`);
  }

  obtenerTotalInspecciones(): Observable<any> {
    return this.http.get(`${this.apiUrl}/Inspecciones`);
  }
}