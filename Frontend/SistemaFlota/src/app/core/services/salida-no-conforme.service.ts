import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface CrearSalidaNoConformeDto {
  ordenProduccion: string;
  referencia?: string;
  cantidadKg?: number;
  cliente?: string;
  linea?: string;
  material?: string;
  proceso?: string;
  descripcionSalida?: string;
  tipoDefecto?: string;
  impacto?: string;
  causaRaiz?: string;
  cantidadReportadaKg?: number;
  unidadCantidadReportada?: string;
  firmaReporta?: string;
  nombreReporta?: string;
}

export interface TratamientoSncDto {
  tratamientoAdoptado?: string;
  descripcionTratamiento?: string;
  fechaTratamiento?: string;
  firmaTratamiento?: string;
}

export interface VerificacionSncDto {
  verificacionCumplimiento?: string;
  requiereInformacionCliente?: boolean;
  motivoInformacionCliente?: string;
  aceptacionBajoConcesion?: boolean;
  detalleAceptacionConcesion?: string;
  firmaVerificacion?: string;
  revisadoPor?: string;
}

@Injectable({ providedIn: 'root' })
export class SalidaNoConformeService {
  private apiUrl = `${environment.apiUrl}/SalidaNoConforme`;

  constructor(private http: HttpClient) {}

  getRegistros(desde?: string, hasta?: string, referencia?: string, material?: string, ordenProduccion?: string, tipoDefecto?: string, proceso?: string): Observable<any[]> {
    let params = new HttpParams();
    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);
    if (referencia) params = params.set('referencia', referencia);
    if (material) params = params.set('material', material);
    if (ordenProduccion) params = params.set('ordenProduccion', ordenProduccion);
    if (tipoDefecto) params = params.set('tipoDefecto', tipoDefecto);
    if (proceso) params = params.set('proceso', proceso);
    return this.http.get<any[]>(this.apiUrl, { params });
  }

  getPorId(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  crear(fd: FormData): Observable<any> {
    return this.http.post<any>(this.apiUrl, fd);
  }

  registrarTratamiento(id: number, dto: TratamientoSncDto): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/tratamiento`, dto);
  }

  cerrar(id: number, fd: FormData): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/cerrar`, fd);
}

  eliminar(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  kgDelMes(mes: number, anio: number): Observable<any> {
    let params = new HttpParams().set('mes', mes).set('anio', anio);
    return this.http.get<any>(`${this.apiUrl}/kg-del-mes`, { params });
  }

  sumaFiltrada(desde?: string, hasta?: string, referencia?: string, material?: string, ordenProduccion?: string, tipoDefecto?: string, proceso?: string): Observable<any> {
    let params = new HttpParams();
    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);
    if (referencia) params = params.set('referencia', referencia);
    if (material) params = params.set('material', material);
    if (ordenProduccion) params = params.set('ordenProduccion', ordenProduccion);
    if (tipoDefecto) params = params.set('tipoDefecto', tipoDefecto);
    if (proceso) params = params.set('proceso', proceso);
    return this.http.get<any>(`${this.apiUrl}/suma-filtrada`, { params });
  }

  subirEvidencias(id: number, paso: string, fotos: File[]): Observable<any> {
    const fd = new FormData();
    fd.append('paso', paso);
    fotos.forEach(f => fd.append('fotos', f));
    return this.http.post<any>(`${this.apiUrl}/${id}/evidencias`, fd);
  }

  getEvidencias(id: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${id}/evidencias`);
  }
}