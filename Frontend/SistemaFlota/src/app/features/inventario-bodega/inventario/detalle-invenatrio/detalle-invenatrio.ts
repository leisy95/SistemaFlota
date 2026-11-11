import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InventarioBodega } from '../../../../core/models/inventario-bodega/inventario-bodega.models';
import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-detalle-invenatrio',
  standalone: true,
  imports: [
    CommonModule,
    DecimalPipe
  ],
  templateUrl: './detalle-invenatrio.html',
  styleUrl: './detalle-invenatrio.scss',
})
export class DetalleInvenatrio {
  constructor(
    @Inject(MAT_DIALOG_DATA) public item: InventarioBodega,
    private dialogRef: MatDialogRef<DetalleInvenatrio>
  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }
}