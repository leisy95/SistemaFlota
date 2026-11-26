import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';

@Component({
  selector: 'app-informacion-orden-prod',
  standalone: true,
  imports: [CommonModule, MatDialogModule],
  templateUrl: './informacion-orden-prod.html',
  styleUrl: './informacion-orden-prod.scss',
})
export class InformacionOrdenProd {

  referencia = '';

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { referencia: string },
    private dialogRef: MatDialogRef<InformacionOrdenProd>
  ) {
    this.referencia = data.referencia;
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}