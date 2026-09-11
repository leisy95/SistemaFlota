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
      next: resp => this.recepcion = resp,
      error: () => {
        this.toastr.error('No fue posible cargar la recepción.');
        this.dialogRef.close();
      }
    });
  }

  confirmarRecepcion(): void {
    if (this.confirmando) return;

    const dialogRef = this.dialog.open(DialogConfirmacion, {
      width: '450px',
      maxWidth: '90vw',
      disableClose: true,
      data: {
        titulo: '¿Confirmar recepción?',
        mensaje: 'Al confirmar esta recepción, las cantidades recibidas serán ingresadas al inventario. Esta acción no podrá deshacerse desde este proceso.',
        textoConfirmar: 'Sí, confirmar e ingresar',
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
            'Recepción confirmada e inventario actualizado.',
            'Recepción'
          );

          this.dialogRef.close(true);
        },
        error: error => {
          this.confirmando = false;

          this.toastr.error(
            error.error?.mensaje ?? 'No fue posible confirmar la recepción.',
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