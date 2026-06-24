import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FormatoFGC008Service {
  private apiUrl = `${environment.apiUrl}/FormatoFGC008`;
  constructor(private http: HttpClient) {}

  getRegistros(desde?: string, hasta?: string, op?: string): Observable<any[]> {
    let params = new HttpParams();
    if (desde) params = params.set('desde', desde);
    if (hasta) params = params.set('hasta', hasta);
    if (op) params = params.set('op', op);
    return this.http.get<any[]>(this.apiUrl, { params });
  }

  crearRegistro(fd: FormData): Observable<any> { return this.http.post(this.apiUrl, fd); }
  eliminarRegistro(id: number): Observable<any> { return this.http.delete(`${this.apiUrl}/${id}`); }
  editarRegistro(id: number, fd: FormData): Observable<any> { return this.http.put(`${this.apiUrl}/${id}`, fd); }
}
