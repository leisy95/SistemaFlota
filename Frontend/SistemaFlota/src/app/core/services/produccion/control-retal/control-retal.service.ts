
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { ConsultarOrdenControlRetalDto } from '../../../models/produccion/control-retal/control-retal.model';

@Injectable({ providedIn: 'root' })
export class ControlRetalService {

    private apiUrl = `${environment.apiUrl}/ControlRetal`;

    constructor(private http: HttpClient) { }

    consultarOrden(numop: number): Observable<ConsultarOrdenControlRetalDto> {
        return this.http.get<ConsultarOrdenControlRetalDto>(
            `${this.apiUrl}/orden/${numop}`
        );
    }
}
