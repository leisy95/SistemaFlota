import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SeguimientoRrhh, CrearSeguimientoRrhhDto } from '../models/seguimientos-rrhh.model';

@Injectable({ providedIn: 'root' })
export class SeguimientosRrhhService {
  private readonly apiUrl = `${environment.apiUrl}/seguimientosrrhh`;

  constructor(private http: HttpClient) {}

  getAll(filtros?: {
    area?: string; estado?: string; prioridad?: string; mes?: number; anio?: number;
  }): Observable<SeguimientoRrhh[]> {
    let params = new HttpParams();
    if (filtros?.area)      params = params.set('area',      filtros.area);
    if (filtros?.estado)    params = params.set('estado',    filtros.estado);
    if (filtros?.prioridad) params = params.set('prioridad', filtros.prioridad);
    if (filtros?.mes)       params = params.set('mes',       filtros.mes.toString());
    if (filtros?.anio)      params = params.set('anio',      filtros.anio.toString());
    return this.http.get<SeguimientoRrhh[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<SeguimientoRrhh> {
    return this.http.get<SeguimientoRrhh>(`${this.apiUrl}/${id}`);
  }

  crear(dto: CrearSeguimientoRrhhDto, fotosEvidencia: File[], fotosSeguimiento: File[]): Observable<SeguimientoRrhh> {
    return this.http.post<SeguimientoRrhh>(this.apiUrl, this.buildFormData(dto, fotosEvidencia, fotosSeguimiento));
  }

  actualizar(id: number, dto: CrearSeguimientoRrhhDto, fotosEvidencia: File[], fotosSeguimiento: File[]): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, this.buildFormData(dto, fotosEvidencia, fotosSeguimiento));
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  eliminarFoto(fotoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/foto/${fotoId}`);
  }

  getFotoUrl(nombreArchivo: string): string {
    return `${environment.fotosUrl}/seguimientos-rrhh/${nombreArchivo}`;
  }

  private buildFormData(dto: any, fotosEvidencia: File[], fotosSeguimiento: File[]): FormData {
    const fd = new FormData();
    Object.keys(dto).forEach(key => {
      const val = dto[key];
      if (val !== null && val !== undefined) fd.append(key, val.toString());
    });
    fotosEvidencia.forEach(f   => fd.append('fotosEvidencia',   f, f.name));
    fotosSeguimiento.forEach(f => fd.append('fotosSeguimiento', f, f.name));
    return fd;
  }
}