import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-acciones-recepcion-dialog',
  standalone: true,
  imports: [],
  templateUrl: './acciones-recepcion-dialog.html',
  styleUrl: './acciones-recepcion-dialog.scss',
})
export class AccionesRecepcionDialog {
  constructor(
    private dialogRef: MatDialogRef<AccionesRecepcionDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  seleccionar(accion: string): void {
    this.dialogRef.close(accion);
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
