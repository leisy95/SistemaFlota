import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-detalle-movimiento',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
  ],
  templateUrl: './detalle-movimiento.html',
  styleUrl: './detalle-movimiento.scss',
})
export class DetalleMovimiento {
  constructor(
    @Inject(MAT_DIALOG_DATA) public movimiento: any
  ) { }
}
