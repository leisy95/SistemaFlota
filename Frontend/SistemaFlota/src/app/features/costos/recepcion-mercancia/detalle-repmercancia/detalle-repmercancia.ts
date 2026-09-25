import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { RecepcionMercanciaService } from '../../../../core/services/costos/recepcionmercancia/recepcionmercancia.service';
import { DialogConfirmacion } from '../../../../shared/dialog-confirmacion/dialog-confirmacion';

@Component({
  selector: 'app-detalle-repmercancia',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './detalle-repmercancia.html',
  styleUrl: './detalle-repmercancia.scss',
})
export class DetalleRepmercancia implements OnInit {
  recepcion: any;
  entregas: any[] = [];
  hayEntregasPendientes = false;
  confirmando = false;

  constructor(
    private dialogRef: MatDialogRef<DetalleRepmercancia>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private recepcionService: RecepcionMercanciaService,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.obtenerRecepcion();
  }

  obtenerRecepcion(): void {
    this.recepcionService.obtenerPorId(this.data.id).subscribe({
      next: resp => {
        this.recepcion = resp;
        this.agruparEntregas();
      },
      error: () => {
        this.toastr.error('No fue posible cargar la recepción.');
        this.dialogRef.close();
      }
    });
  }

  agruparEntregas(): void {
    const grupos = new Map<number, any>();

    for (const detalle of this.recepcion.detalles ?? []) {
      if (!grupos.has(detalle.numeroEntrega)) {
        grupos.set(detalle.numeroEntrega, {
          numeroEntrega: detalle.numeroEntrega,
          fechaEntrega: detalle.fechaEntrega,
          procesadoInventario: true,
          detalles: [],
          totalKg: 0,
          totalBultos: 0
        });
      }

      const entrega = grupos.get(detalle.numeroEntrega);

      entrega.detalles.push(detalle);
      entrega.totalKg += detalle.cantidadRecibida;
      entrega.totalBultos += detalle.bultosRecibidos;

      if (!detalle.procesadoInventario) {
        entrega.procesadoInventario = false;
      }
    }

    this.entregas = Array.from(grupos.values())
      .sort((a, b) => a.numeroEntrega - b.numeroEntrega);

    this.hayEntregasPendientes = this.entregas.some(
      entrega => !entrega.procesadoInventario
    );
  }

  confirmarRecepcion(): void {
    if (this.confirmando || !this.hayEntregasPendientes) return;

    const dialogRef = this.dialog.open(DialogConfirmacion, {
      width: '450px',
      maxWidth: '90vw',
      disableClose: true,
      data: {
        titulo: '¿Confirmar recepción?',
        mensaje: 'Las entregas pendientes serán ingresadas al inventario. Las entregas que ya fueron procesadas no se volverán a ingresar.',
        textoConfirmar: 'Sí, ingresar a inventario',
        textoCancelar: 'Cancelar',
        tipo: 'warning'
      }
    });

    dialogRef.afterClosed().subscribe(confirmada => {
      if (!confirmada) return;

      this.confirmando = true;

      this.recepcionService.confirmarRecepcion(this.recepcion.id).subscribe({
        next: () => {
          this.toastr.success(
            'Las entregas pendientes fueron ingresadas al inventario.',
            'Recepción'
          );
          this.dialogRef.close(true);
        },
        error: error => {
          this.confirmando = false;
          this.toastr.error(
            error.error?.mensaje ?? 'No fue posible ingresar la recepción al inventario.',
            'Recepción'
          );
        }
      });
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}