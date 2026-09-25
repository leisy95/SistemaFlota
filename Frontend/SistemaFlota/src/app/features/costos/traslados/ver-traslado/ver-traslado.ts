import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { VerificarOrdenTraslado } from '../verificar-orden-traslado/verificar-orden-traslado';
import { OrdenTraslado } from '../../../../core/models/costos/OrdenesTraslado/orden-traslado.model';
import { DialogConfirmacion, DialogConfirmacionData } from '../../../../shared/dialog-confirmacion/dialog-confirmacion';
import { OrdenTrasladoService } from '../../../../core/services/costos/ordenestraslado/ordentraslado.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-ver-traslado',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './ver-traslado.html',
  styleUrl: './ver-traslado.scss',
})
export class VerTraslado implements OnInit {

  orden: any = null;
  cargando = true;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    private dialogRef: MatDialogRef<VerTraslado>,
    private dialog: MatDialog,
    private toastr: ToastrService,
    private ordenTrasladoService: OrdenTrasladoService
  ) { }

  ngOnInit(): void {
    this.orden = this.data;

    this.cargando = false;

    console.log('Orden recibida:', this.orden);
  }

  cerrar(): void {
    this.dialogRef.close();
  }

  obtenerClaseEstado(estado: string): string {
    switch (estado?.toLowerCase()) {

      case 'completado':
      case 'confirmado':
        return 'completada';

      case 'en proceso':
      case 'verificando':
        return 'proceso';

      case 'pendiente':
        return 'pendiente';

      case 'anulado':
        return 'anulada';

      default:
        return 'pendiente';
    }
  }

  iniciarVerificacion(): void {
    const dialogRef = this.dialog.open(VerificarOrdenTraslado, {
      width: '1000px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      data: this.orden
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.dialogRef.close(resultado);
      }
    });
  }

  confirmarOrden(): void {

    if (this.orden.estado !== 'Verificando') {
      return;
    }

    const dialogRef = this.dialog.open(DialogConfirmacion, {
      width: '450px',
      maxWidth: '95vw',
      disableClose: true,
      data: {
        titulo: 'Confirmar orden de traslado',
        mensaje: `¿Está seguro de confirmar la orden ${this.orden.numeroOrden}?`,
        textoConfirmar: 'Sí, confirmar',
        textoCancelar: 'Cancelar',
        tipo: 'warning'
      } as DialogConfirmacionData
    });

    dialogRef.afterClosed().subscribe(confirmado => {

      if (!confirmado) {
        return;
      }

      this.ordenTrasladoService.confirmar(this.orden.id).subscribe({
        next: () => {

          this.toastr.success(
            `La orden ${this.orden.numeroOrden} fue confirmada correctamente.`,
            'Orden de traslado'
          );

          this.dialogRef.close(true);
        },

        error: error => {
          this.toastr.error(
            error?.error?.mensaje ||
            'No fue posible confirmar la orden.',
            'Error'
          );
        }
      });

    });
  }
}