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
    fechaDesde?: string;
    fechaHasta?: string;
  }): Observable<any> {
    let httpParams = new HttpParams();

    if (params?.pagina != null) {
      httpParams = httpParams.set('pagina', params.pagina.toString());
    }

    if (params?.porPagina != null) {
      httpParams = httpParams.set('porPagina', params.porPagina.toString());
    }

    if (params?.buscar?.trim()) {
      httpParams = httpParams.set('buscar', params.buscar.trim());
    }

    if (params?.fechaDesde) {
      httpParams = httpParams.set('fechaDesde', params.fechaDesde);
    }

    if (params?.fechaHasta) {
      httpParams = httpParams.set('fechaHasta', params.fechaHasta);
    }

    return this.http.get(`${this.apiUrl}/Inspecciones`, {
      params: httpParams
    });
  }

  obtenerDetalle(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/Inspecciones/${id}`);
  }

  obtenerTotalInspecciones(): Observable<any> {
    return this.http.get(`${this.apiUrl}/Inspecciones`);
  }

  // Llamar enpoint imprimir pdf
  descargarPdf(id: number): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/Inspecciones/${id}/pdf`,
      { responseType: 'blob' }
    );
  }

  // Exportar pdf general
  exportarPdfGeneral(params?: {
    buscar?: string;
    fechaDesde?: string;
    fechaHasta?: string;
  }): Observable<Blob> {
    let httpParams = new HttpParams();

    if (params?.buscar) {
      httpParams = httpParams.set('buscar', params.buscar);
    }

    if (params?.fechaDesde) {
      httpParams = httpParams.set('fechaDesde', params.fechaDesde);
    }

    if (params?.fechaHasta) {
      httpParams = httpParams.set('fechaHasta', params.fechaHasta);
    }

    return this.http.get(
      `${this.apiUrl}/Inspecciones/pdf-general`,
      {
        params: httpParams,
        responseType: 'blob'
      }
    );
  }

}