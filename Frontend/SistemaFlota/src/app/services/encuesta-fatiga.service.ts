import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class EncuestaFatigaService {

  private apiUrl = `${environment.apiUrl}/EncuestaFatiga`;

  constructor(private http: HttpClient) {}

  obtenerEncuestas(params?: {
    pagina?: number;
    porPagina?: number;
    buscar?: string;
    resultado?: string;
    conductorId?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params?.pagina)      httpParams = httpParams.set('pagina',      params.pagina.toString());
    if (params?.porPagina)   httpParams = httpParams.set('porPagina',   params.porPagina.toString());
    if (params?.buscar)      httpParams = httpParams.set('buscar',      params.buscar);
    if (params?.resultado)   httpParams = httpParams.set('resultado',   params.resultado);
    if (params?.conductorId) httpParams = httpParams.set('conductorId', params.conductorId.toString());
    return this.http.get(this.apiUrl, { params: httpParams });
  }

  obtenerPorConductor(conductorId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/conductor/${conductorId}`);
  }

  obtenerEstadisticas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estadisticas`);
  }

  crear(dto: any): Observable<any> {
    return this.http.post(this.apiUrl, dto);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}