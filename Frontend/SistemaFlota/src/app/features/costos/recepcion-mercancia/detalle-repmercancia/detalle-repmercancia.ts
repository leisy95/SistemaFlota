import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { RecepcionMercanciaService } from '../../../../core/services/costos/recepcionmercancia/recepcionmercancia.service';

@Component({
  selector: 'app-detalle-repmercancia',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './detalle-repmercancia.html',
  styleUrl: './detalle-repmercancia.scss',
})
export class DetalleRepmercancia implements OnInit {

  recepcion: any;
  confirmando = false;

  constructor(
    private dialogRef: MatDialogRef<DetalleRepmercancia>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private recepcionService: RecepcionMercanciaService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.obtenerRecepcion();
  }

  obtenerRecepcion(): void {
    this.recepcionService.obtenerPorId(this.data.id).subscribe({
      next: resp => this.recepcion = resp,
      error: () => {
        this.toastr.error('No fue posible cargar la recepción.');
        this.dialogRef.close();
      }
    });
  }

  confirmarRecepcion(): void {
    this.confirmando = true;

    this.recepcionService.confirmarRecepcion(this.recepcion.id).subscribe({
      next: () => {
        this.toastr.success('Recepción confirmada e inventario actualizado.');
        this.dialogRef.close(true);
      },
      error: error => {
        this.confirmando = false;
        this.toastr.error(error.error?.mensaje ?? 'No fue posible confirmar la recepción.');
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }

}