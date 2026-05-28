import { Injectable }
from '@angular/core';

import { HttpClient }
from '@angular/common/http';

import { Observable }
from 'rxjs';

import { environment }
from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class InspeccionesService {

  private apiUrl =
    environment.apiUrl;

  constructor(

    private http:
      HttpClient

  ) {}

  // GUARDAR INSPECCIÓN

  guardarInspeccion(
    formData: FormData
  ): Observable<any> {

    return this.http.post(

      `${this.apiUrl}/Inspecciones`,

      formData

    );

  }

  // OBTENER HISTORIAL

  obtenerHistorial() {

    return this.http.get<any[]>(

      `${this.apiUrl}/Inspecciones`

    );

  }

  // OBTENER DETALLE

  obtenerDetalle(
    id: number
  ) {

    return this.http.get(

      `${this.apiUrl}/Inspecciones/${id}`

    );

  }

  obtenerTotalInspecciones() {

  return this.http.get<any[]>(

    `${this.apiUrl}/Inspecciones`

  );

}

}