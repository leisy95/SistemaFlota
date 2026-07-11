import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuditoriaService {

  private apiUrl = `${environment.apiUrl}/Auditoria`;

  constructor(private http: HttpClient) {}

  obtenerAuditorias(filtros: any = {}): Observable<any> {
    const params = new URLSearchParams();
    if (filtros.usuario)    params.append('usuario',    filtros.usuario);
    if (filtros.modulo)     params.append('modulo',     filtros.modulo);
    if (filtros.accion)     params.append('accion',     filtros.accion);
    if (filtros.resultado)  params.append('resultado',  filtros.resultado);
    if (filtros.fechaDesde) params.append('fechaDesde', filtros.fechaDesde);
    if (filtros.fechaHasta) params.append('fechaHasta', filtros.fechaHasta);
    if (filtros.pagina)     params.append('pagina',     filtros.pagina);
    if (filtros.porPagina)  params.append('porPagina',  filtros.porPagina);

    return this.http.get(`${this.apiUrl}?${params.toString()}`);
  }

  obtenerEstadisticas(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estadisticas`);
  }

  limpiarRegistros(dias: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/limpiar?dias=${dias}`);
  }
}