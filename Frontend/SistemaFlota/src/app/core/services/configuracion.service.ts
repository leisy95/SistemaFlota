import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfiguracionService {

  private apiUrl = `${environment.apiUrl}/Configuracion`;

  constructor(private http: HttpClient) {}

  obtenerConfiguracion(): Observable<any> {
    return this.http.get(this.apiUrl);
  }

  guardarConfiguracion(formData: FormData): Observable<any> {
    return this.http.post(this.apiUrl, formData);
  }
}