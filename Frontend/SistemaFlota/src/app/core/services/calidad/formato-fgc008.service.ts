import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FormatoFGC008Service {
  private apiUrl = `${environment.apiUrl}/FormatoFGC008`;
  constructor(private http: HttpClient) { }

  private filtros(desde?: string, hasta?: string, op?: string): HttpParams {
    let params = new HttpParams();

    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);
    if (op?.trim()) params = params.set('op', op.trim());

    return params;
  }

  // Todas las filas del filtro (lo usa el PDF)
  getRegistros(desde?: string, hasta?: string, op?: string): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl, { params: this.filtros(desde, hasta, op) });
  }

  // Una página de la lista
  getPaginado(desde?: string, hasta?: string, op?: string, pagina = 1, porPagina = 25): Observable<any> {
    const params = this.filtros(desde, hasta, op).set('pagina', pagina).set('porPagina', porPagina);
    return this.http.get<any>(`${this.apiUrl}/paginado`, { params });
  }

  // Tarjetas de resumen de todo el filtro
  getResumen(desde?: string, hasta?: string, op?: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/resumen`, { params: this.filtros(desde, hasta, op) });
  }

  // Generar PDF desde el backend
  generarPdf(desde?: string, hasta?: string, op?: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/pdf`, {
      params: this.filtros(desde, hasta, op),
      responseType: 'blob'
    });
  }

  crearRegistro(fd: FormData): Observable<any> { return this.http.post(this.apiUrl, fd); }
  eliminarRegistro(id: number): Observable<any> { return this.http.delete(`${this.apiUrl}/${id}`); }
  editarRegistro(id: number, fd: FormData): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, fd); }
  buscarOP(op: string): Observable<any> { return this.http.get(`${this.apiUrl}/op/${op}`); }
  subirFotos(id: number, fotos: File[]): Observable<any> {
    const fd = new FormData();
    fotos.forEach(f => fd.append('fotos', f, f.name));
    return this.http.post(`${this.apiUrl}/${id}/fotos`, fd);
  }

  eliminarFoto(id: number, fotoId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}/fotos/${fotoId}`);
  }
  sincronizar(op: string): Observable<any> { return this.http.post<any>(`${this.apiUrl}/sincronizar/${op}`, {}); }
}