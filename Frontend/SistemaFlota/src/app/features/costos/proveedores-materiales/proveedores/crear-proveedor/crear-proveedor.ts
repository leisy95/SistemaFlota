import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';


@Component({
  selector: 'app-crear-proveedor',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
  ],
  templateUrl: './crear-proveedor.html',
  styleUrl: './crear-proveedor.scss',
})
export class CrearProveedor {

  constructor(
    private dialogRef: MatDialogRef<CrearProveedor>,
    @Inject(MAT_DIALOG_DATA) public data: any

  ) { }

  cerrar(): void {
    this.dialogRef.close();
  }

  guardar(): void {
    // Aquí irá la lógica para guardar
    this.dialogRef.close(true);
  }

}
