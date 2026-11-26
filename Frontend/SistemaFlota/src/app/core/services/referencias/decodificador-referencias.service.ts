import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CatalogosReferencia,
  DescripcionInterpretada,
  ReferenciaDecodificada
} from '../../models/referencias/referencia-decodificada.model';


@Injectable({ providedIn: 'root' })
export class DecodificadorReferenciasService {
  private readonly base = `${environment.apiUrl}/DecodificadorReferencias`;
  private catalogos$?: Observable<CatalogosReferencia>;

  constructor(private http: HttpClient) {}

  decodificar(codigo: string): Observable<ReferenciaDecodificada> {
    return this.http.post<ReferenciaDecodificada>(`${this.base}/decodificar`, { codigo });
  }

  decodificarLote(codigos: string[]): Observable<ReferenciaDecodificada[]> {
    if (!codigos.length) return of([]);
    return this.http.post<ReferenciaDecodificada[]>(`${this.base}/decodificar-lote`, { codigos });
  }

  interpretarDescripcion(descripcion: string): Observable<DescripcionInterpretada> {
    return this.http.post<DescripcionInterpretada>(`${this.base}/interpretar-descripcion`, { descripcion });
  }

  interpretarDescripcionesLote(descripciones: string[]): Observable<DescripcionInterpretada[]> {
    if (!descripciones.length) return of([]);
    return this.http.post<DescripcionInterpretada[]>(`${this.base}/interpretar-descripciones-lote`, { descripciones });
  }

  // Los catálogos casi no cambian: se piden una sola vez y se reutilizan.
  catalogos(): Observable<CatalogosReferencia> {
    this.catalogos$ ??= this.http
      .get<CatalogosReferencia>(`${this.base}/catalogos`)
      .pipe(shareReplay(1));
    return this.catalogos$;
  }
}