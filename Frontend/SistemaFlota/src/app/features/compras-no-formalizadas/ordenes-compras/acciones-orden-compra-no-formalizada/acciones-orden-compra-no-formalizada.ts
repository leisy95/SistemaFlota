import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogRef
} from '@angular/material/dialog';
import { OrdenCompraNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-compras/OrdenCompraNoFormalizadaService';
import { PermisosService } from '../../../../core/services/permisos.service';
import { DetalleOrdenCompraNoFormalizada } from '../detalle-orden-compra-no-formalizada/detalle-orden-compra-no-formalizada';
import { CrearOrdenCompra } from '../crear-orden-compra/crear-orden-compra';


@Component({
  selector: 'app-acciones-orden-compra-no-formalizada',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './acciones-orden-compra-no-formalizada.html',
  styleUrl: './acciones-orden-compra-no-formalizada.scss',
})
export class AccionesOrdenCompraNoFormalizada {

  constructor(
    private dialogRef: MatDialogRef<AccionesOrdenCompraNoFormalizada>,
    @Inject(MAT_DIALOG_DATA) public orden: any,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraNoFormalizadaService,
    public permisos: PermisosService
  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }

  accion(nombre: string): void {

    if (nombre === 'detalle') {

      this.dialogRef.close();

      this.dialog.open(DetalleOrdenCompraNoFormalizada, {
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