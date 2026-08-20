import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Inject, Input, Output } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface DialogConfirmacionData {
  titulo: string;
  mensaje: string;
  textoConfirmar?: string;
  textoCancelar?: string;
  tipo?: 'warning' | 'danger' | 'info';
}

@Component({
  selector: 'app-dialog-confirmacion',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './dialog-confirmacion.html',
  styleUrl: './dialog-confirmacion.scss',
})
export class DialogConfirmacion {

  constructor(
    private dialogRef: MatDialogRef<DialogConfirmacion>,
    @Inject(MAT_DIALOG_DATA) public data: DialogConfirmacionData
  ) { }

  cancelar(): void {
    this.dialogRef.close(false);
  }

  confirmar(): void {
    this.dialogRef.close(true);
  }
}
