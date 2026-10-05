import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-acciones-recepcion-dialog-no-formalizada',
  standalone: true,
  imports: [],
  templateUrl: './acciones-recepcion-dialog-no-formalizada.html',
  styleUrl: './acciones-recepcion-dialog-no-formalizada.scss',
})
export class AccionesRecepcionDialogNoFormalizada {

  constructor(
    private dialogRef: MatDialogRef<AccionesRecepcionDialogNoFormalizada>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  seleccionar(accion: string): void {
    this.dialogRef.close(accion);
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
