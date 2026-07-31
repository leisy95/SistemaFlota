import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { CrearOrdenCompra } from '../crear-orden-compra/crear-orden-compra';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';
import { DetalleOrdenCompra } from '../detalle-orden-compra/detalle-orden-compra';

@Component({
  selector: 'app-acciones-orden-compra',
  imports: [],
  templateUrl: './acciones-orden-compra.html',
  styleUrl: './acciones-orden-compra.scss',
})
export class AccionesOrdenCompra {
  constructor(
    private dialogRef: MatDialogRef<AccionesOrdenCompra>,
    @Inject(MAT_DIALOG_DATA) public orden: any,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraService
  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }

  accion(nombre: string): void {

    if (nombre === 'detalle') {

      this.dialogRef.close();

      this.dialog.open(DetalleOrdenCompra, {
        width: '1200px',
        maxWidth: '95vw',
        maxHeight: '95vh',
        disableClose: true,
        autoFocus: false,
        panelClass: 'orden-compra-dialog',
        data: {
          id: this.orden.id
        }
      });

      return;
    }

    if (nombre === 'editar') {

      this.dialogRef.close();

      this.dialog.open(CrearOrdenCompra, {
        width: '1200px',
        maxWidth: '95vw',
        maxHeight: '95vh',
        disableClose: true,
        autoFocus: false,
        panelClass: 'orden-compra-dialog',
        data: {
          modo: 'editar',
          id: this.orden.id
        }
      });

      return;
    }

    if (nombre === 'recibir') {

    }

    if (nombre === 'imprimir') {
      this.generarPdf();
      return;
    }

    this.dialogRef.close(nombre);
  }

  generarPdf(): void {

    this.ordenCompraService
      .generarPdf(this.orden.id)
      .subscribe({
        next: (blob) => {

          const url = window.URL.createObjectURL(blob);
          window.open(url, '_blank');

          this.dialogRef.close();
        },
        error: (err) => {
          console.error(err);
        }
      });

  }
}
