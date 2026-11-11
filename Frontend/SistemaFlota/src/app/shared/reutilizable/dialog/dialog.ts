import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Inject, Input, Optional, Output } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {

  // Propiedades para cuando se usa como etiqueta en el HTML ([visible], etc.)
  @Input() visible: boolean = false;
  @Input() titulo: string = 'Aviso';
  @Input() mensaje: string = '';
  @Input() tipo: string = 'info';
  @Input() textoBoton: string = 'Aceptar';
  @Input() textoCancelar: string = 'Cancelar';
  @Input() mostrarCancelar: boolean = false;

  @Output() cerrar = new EventEmitter<void>();
  @Output() respuesta = new EventEmitter<boolean>();

  // Detecta si se abrió mediante MatDialog (servicio) o mediante HTML (*ngIf)
  private esMatDialog: boolean = false;

  constructor(
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any,
    @Optional() private dialogRef: MatDialogRef<Dialog>
  ) {
    // Si viene por MatDialog, sobrescribimos los valores con los datos inyectados
    if (data) {
      this.esMatDialog = true;
      this.visible = true;
      this.titulo = data.titulo || 'Aviso';
      this.mensaje = data.mensaje || '';
      this.tipo = data.tipo || 'info';
      this.textoBoton = data.textoBoton || 'Aceptar';
      this.textoCancelar = data.textoCancelar || 'Cancelar';
      this.mostrarCancelar = data.mostrarCancelar || false;
    }
  }

  aceptar(): void {
    if (this.esMatDialog && this.dialogRef) {
      this.dialogRef.close(true);
    } else {
      this.visible = false;
      this.cerrar.emit();
      this.respuesta.emit(true);
    }
  }

  cancelar(): void {
    if (this.esMatDialog && this.dialogRef) {
      this.dialogRef.close(false);
    } else {
      this.visible = false;
      this.cerrar.emit();
      this.respuesta.emit(false);
    }
  }

  cerrarDialog(): void {
    this.cancelar();
  }
}