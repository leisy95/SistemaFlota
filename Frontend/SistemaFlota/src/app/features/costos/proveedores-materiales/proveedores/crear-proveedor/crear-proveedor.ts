import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { Proveedor } from '../../../../../core/models/costos/proveedores/proveedores.model';
import { ProveedorService } from '../../../../../core/services/costos/proveedores/proveedor.service';


@Component({
  selector: 'app-crear-proveedor',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    FormsModule
  ],
  templateUrl: './crear-proveedor.html',
  styleUrl: './crear-proveedor.scss',
})
export class CrearProveedor {

  proveedor: Proveedor = {
    nombre: '',
    nit: '',
    contacto: '',
    telefono: '',
    correoElectronico: ''
  };

  constructor(
    private proveedorService: ProveedorService,
    private dialogRef: MatDialogRef<CrearProveedor>,
    @Inject(MAT_DIALOG_DATA) public data: Proveedor | null

  ) {
    if (this.data) {
      this.proveedor = { ...this.data };
      console.log(this.proveedor);
    }
  }

  cerrar(): void {
    this.dialogRef.close();
  }


  guardar(): void {

    if (this.proveedor.idProveedor) {

      // EDITAR
      this.proveedorService
        .actualizar(
          this.proveedor.idProveedor,
          this.proveedor
        )
        .subscribe({
          next: () => {
            this.dialogRef.close(true);
          },

          error: (error) => {
            console.error(
              'Error al actualizar proveedor',
              error
            );
          }
        });
    } else {

      // CREAR
      this.proveedorService
        .crear(this.proveedor)
        .subscribe({

          next: (proveedor) => {
            this.dialogRef.close(proveedor);
          },

          error: (error) => {
            console.error(
              'Error al crear proveedor',
              error
            );
          }
        });
    }
  }

}
