import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FormatosCalidadService {
  private api = `${environment.apiUrl}/FormatosCalidad`;

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  getTipos() {
    return this.http.get<any[]>(`${this.api}/tipos`, { headers: this.headers });
  }

  getCaracteristicas(codigo: string) {
    return this.http.get<any>(`${this.api}/tipos/${codigo}/caracteristicas`, { headers: this.headers });
  }

  getRegistros(codigo: string, desde?: string, hasta?: string, op?: string) {
    let params = new HttpParams().set('codigo', codigo);
    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);
    if (op) params = params.set('op', op);
    return this.http.get<any[]>(`${this.api}/registros`, { headers: this.headers, params });
  }

  crearRegistro(dto: any) {
    return this.http.post<any>(`${this.api}/registros`, dto, { headers: this.headers });
  }

  editarRegistro(id: number, dto: any) {
    return this.http.put<any>(`${this.api}/registros/${id}`, dto, { headers: this.headers });
  }
  liberarRegistro(id: number, dto: any) {
    return this.http.put<any>(`${this.api}/registros/${id}/liberar`, dto, { headers: this.headers });
  }

  eliminarRegistro(id: number) {
    return this.http.delete<void>(`${this.api}/registros/${id}`, { headers: this.headers });
  }

  buscarOP(op: string, tipoFormatoId: number) {
    return this.http.get<any>(`${this.api}/registros/op/${op}?tipoFormatoId=${tipoFormatoId}`, { headers: this.headers });
  }

  buscarDesperdicioPorOrden(op: string, tipoFormatoId: number) {
    return this.http.get<any>(`${this.api}/desperdicio-orden?op=${encodeURIComponent(op)}&tipoFormatoId=${tipoFormatoId}`, { headers: this.headers });
}

  buscarMejorRendimiento(referencia: string, maquina?: string) {
    let url = `${this.api}/mejor-rendimiento?referencia=${encodeURIComponent(referencia)}`;
    if (maquina) url += `&maquina=${encodeURIComponent(maquina)}`;
    return this.http.get<any>(url, { headers: this.headers });
  }
  
  constructor(private http: HttpClient) {}
}