import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class EstadoListadoService {

    private estados: Record<string, any> = {};

    guardar(clave: string, estado: any): void {
        this.estados[clave] = { ...estado };
    }

    obtener(clave: string): any {
        return this.estados[clave];
    }

    limpiar(clave: string): void {
        delete this.estados[clave];
    }

}