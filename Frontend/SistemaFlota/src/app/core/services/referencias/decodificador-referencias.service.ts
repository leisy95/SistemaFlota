import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, shareReplay } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CatalogosReferencia,
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

  
  catalogos(): Observable<CatalogosReferencia> {
    this.catalogos$ ??= this.http
      .get<CatalogosReferencia>(`${this.base}/catalogos`)
      .pipe(shareReplay(1));
    return this.catalogos$;
  }
  }