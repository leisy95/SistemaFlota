import { Injectable } from '@angular/core';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Dialog } from '../../../shared/reutilizable/dialog/dialog';

@Injectable({
    providedIn: 'root'
})
export class DialogService {

    constructor(private dialog: MatDialog) { }

    alerta(titulo: string, mensaje: string, tipo: string, mostrarCancelar = false, textoBoton = 'Aceptar', textoCancelar = 'Cancelar'): MatDialogRef<Dialog> {
        return this.dialog.open(Dialog, {
            data: { titulo, mensaje, tipo, mostrarCancelar, textoBoton, textoCancelar }
        });
    }
}