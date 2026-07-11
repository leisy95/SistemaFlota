import {
  Injectable
}
from '@angular/core';

import {
  HttpClient
}
from '@angular/common/http';

import {
  Observable
}
from 'rxjs';

import {
  environment
}
from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class ChecklistService {

  // API

  private apiUrl =

    `${environment.apiUrl}/ChecklistItems`;

  constructor(

    private http:
      HttpClient

  ) {}

  // =========================
  // OBTENER CHECKLIST
  // =========================

  obtenerChecklist(
    tipoVehiculoId: number
  ): Observable<any[]> {

    return this.http.get<any[]>(

      `${this.apiUrl}/${tipoVehiculoId}`

    );

  }

  // =========================
  // CREAR
  // =========================

  crearChecklist(
    data: any
  ) {

    return this.http.post(

      this.apiUrl,

      data

    );

  }

  // =========================
  // OBTENER TODOS
  // =========================

  obtenerTodos() {

    return this.http.get<any[]>(

      this.apiUrl

    );

  }

  // =========================
  // EDITAR
  // =========================

  editarChecklist(

    id: number,

    data: any

  ) {

    return this.http.put(

      `${this.apiUrl}/${id}`,

      data

    );

  }

  // =========================
  // ELIMINAR
  // =========================

  eliminarChecklist(
    id: number
  ) {

    return this.http.delete(

      `${this.apiUrl}/${id}`

    );

  }

}