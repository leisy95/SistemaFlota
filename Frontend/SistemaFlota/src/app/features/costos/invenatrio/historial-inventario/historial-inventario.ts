import { Component, Inject } from '@angular/core';
import { AjusteInventarioService } from '../../../../core/services/costos/inventario/ajusteinventario.service';
import { ToastrService } from 'ngx-toastr';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-historial-inventario',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './historial-inventario.html',
  styleUrl: './historial-inventario.scss',
})
export class HistorialInventario {

  historial: any[] = [];

  constructor(
    private service: AjusteInventarioService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<HistorialInventario>,
    @Inject(MAT_DIALOG_DATA) public inventarioId: number
  ) { }

  ngOnInit(): void {
    this.cargarHistorial();
  }

  cargarHistorial(): void {
    this.service.obtenerHistorial(this.inventarioId).subscribe({
      next: resp => this.historial = resp,
      error: () => {
        this.toastr.error('No fue posible cargar el historial.');
        this.dialogRef.close();
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }

}