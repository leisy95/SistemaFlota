import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { OrdenTrasladoNoFormalizada } from '../../../../core/models/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizado.models';
import { OrdenTrasladoNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizada.service';
import { VerificarOrdenTrasladoNoFormalizado } from '../verificar-orden-traslado-no-formalizado/verificar-orden-traslado-no-formalizado';

@Component({
  selector: 'app-ver-traslado-no-formalizado',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ver-traslado-no-formalizado.html',
  styleUrl: './ver-traslado-no-formalizado.scss',
})
export class VerTrasladoNoFormalizado {
  orden: OrdenTrasladoNoFormalizada;
  cargando = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: OrdenTrasladoNoFormalizada,
    private dialogRef: MatDialogRef<VerTrasladoNoFormalizado>,
    private dialog: MatDialog,
    private ordenTrasladoService: OrdenTrasladoNoFormalizadaService,
    private toastr: ToastrService
  ) {
    this.orden = data;
  }

  cerrar(): void {
    this.dialogRef.close();
  }

  obtenerClaseEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'completado':
      case 'confirmado': return 'completada';
      case 'en proceso':
      case 'verificando': return 'proceso';
      case 'pendiente': return 'pendiente';
      case 'anulado': return 'anulada';
      default: return 'pendiente';
    }
  }

  iniciarVerificacion(): void {
    const dialogRef = this.dialog.open(VerificarOrdenTrasladoNoFormalizado, {
      width: '1000px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      data: this.orden
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.orden = resultado;
        this.dialogRef.close(resultado);
      }
    });
  }

  confirmarOrden(): void {
    if (this.orden.estado !== 'Verificando') return;

    this.ordenTrasladoService.confirmar(this.orden.id).subscribe({
      next: (respuesta) => {
        this.orden = respuesta;
        this.toastr.success(
          `La orden ${respuesta.numeroOrden} fue confirmada correctamente.`,
          'Orden de traslado no formalizada'
        );
        this.dialogRef.close(respuesta);
      },
      error: (error) => {
        this.toastr.error(
          error?.error?.mensaje ||
          error?.error?.message ||
          'No fue posible confirmar la orden.',
          'Error'
        );
      }
    });
  }
}