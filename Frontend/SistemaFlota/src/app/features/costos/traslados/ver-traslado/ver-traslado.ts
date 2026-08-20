import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

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
    private dialogRef: MatDialogRef<VerTraslado>
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
    console.log('Iniciar verificación:', this.orden);
  }

  confirmarOrden(): void {
    console.log('Confirmar orden:', this.orden);
  }
}