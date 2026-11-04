import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './dialog.html',
  styleUrl: './dialog.scss',
})
export class Dialog {

  @Input() visible = false;

  @Input() titulo = '';

  @Input() mensaje = '';

  @Input() tipo: 'success' | 'warning' | 'error' | 'info' = 'info';

  @Input() textoBoton = 'Aceptar';

  @Output() cerrar = new EventEmitter<void>();

  cerrarDialog() {
    this.cerrar.emit();
  }

}