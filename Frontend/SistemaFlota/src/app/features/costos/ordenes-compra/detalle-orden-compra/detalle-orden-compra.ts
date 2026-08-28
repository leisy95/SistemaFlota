import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';
import { ToastrService } from 'ngx-toastr';
import { OrdenCompraResponse } from '../../../../core/models/costos/ordenCompra/ordencompra-response.model';
import { CommonModule } from '@angular/common';
import { PermisosService } from '../../../../core/services/permisos.service';

@Component({
  selector: 'app-detalle-orden-compra',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule
  ],
  templateUrl: './detalle-orden-compra.html',
  styleUrl: './detalle-orden-compra.scss',
})
export class DetalleOrdenCompra implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<DetalleOrdenCompra>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private ordenCompraService: OrdenCompraService,
    private toastr: ToastrService,
    public permisos: PermisosService
  ) { }

  cargando = true;

  orden!: OrdenCompraResponse;

  ngOnInit(): void {
    this.obtenerOrden();
  }

  obtenerOrden(): void {

    this.cargando = true;

    this.ordenCompraService
      .obtenerPorId(this.data.id)
      .subscribe({

        next: (respuesta) => {

          this.orden = respuesta;
          this.cargando = false;
        },

        error: () => {

          this.cargando = false;
          this.toastr.error(
            'No fue posible cargar la orden.',
            'Error'
          );
          this.dialogRef.close();
        }
      });
  }

  imprimirPdf(): void {

    if (!this.orden) {
      return;
    }

    this.ordenCompraService
      .generarPdf(this.orden.id)
      .subscribe({

        next: (blob) => {

          const url = window.URL.createObjectURL(blob);
          window.open(url, '_blank');
          setTimeout(() => {
            URL.revokeObjectURL(url);
          }, 3000);
        },

        error: () => {

          this.toastr.error(
            'No fue posible generar el PDF.',
            'Error'
          );
        }
      });
  }

  enviarCorreo(): void {
    if (!this.orden) return;

    this.ordenCompraService.enviarCorreo(this.orden.id).subscribe({
      next: (respuesta) => {
        this.toastr.success(
          respuesta?.mensaje || 'La orden de compra fue enviada correctamente por correo.',
          'Correo enviado'
        );
      },
      error: (error) => {
        const mensaje = error?.error?.mensaje || 'No fue posible enviar la orden por correo.';
        this.toastr.error(mensaje, 'Error');
      }
    });
  }

  get totalKg(): number {

    if (!this.orden) {
      return 0;
    }

    return this.orden.detalles
      .reduce((s, x) => s + x.cantidadKg, 0);
  }

  get totalBultos(): number {

    if (!this.orden) {
      return 0;
    }

    return this.orden.detalles
      .reduce((s, x) => s + x.bultos, 0);
  }

  get totalItems(): number {

    if (!this.orden) {
      return 0;
    }
    return this.orden.detalles.length;
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}