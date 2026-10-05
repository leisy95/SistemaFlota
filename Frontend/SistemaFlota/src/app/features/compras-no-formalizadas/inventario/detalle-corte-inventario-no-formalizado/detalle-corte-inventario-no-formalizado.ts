import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { HistorialCorteDetalleNoFormalizado } from '../../../../core/models/compras-no-formalizadas/inventario/historial-corte-inventario/historial-corte-detalle-no-formalizado.model';

@Component({
  selector: 'app-detalle-corte-inventario-no-formalizado',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './detalle-corte-inventario-no-formalizado.html',
  styleUrl: './detalle-corte-inventario-no-formalizado.scss',
})
export class DetalleCorteInventarioNoFormalizado {
  constructor(
    private dialogRef: MatDialogRef<DetalleCorteInventarioNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public data: HistorialCorteDetalleNoFormalizado

  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }
}
