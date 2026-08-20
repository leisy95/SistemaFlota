import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { HistorialCorteDetalle } from '../../../../core/models/costos/inventario/historialcorteinventario/historialcorteInventario.models';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-detalle-corte-inventario',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './detalle-corte-inventario.html',
  styleUrl: './detalle-corte-inventario.scss',
})
export class DetalleCorteInventario {

  constructor(
    private dialogRef: MatDialogRef<DetalleCorteInventario>,
    @Inject(MAT_DIALOG_DATA) public data: HistorialCorteDetalle
  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }
}
