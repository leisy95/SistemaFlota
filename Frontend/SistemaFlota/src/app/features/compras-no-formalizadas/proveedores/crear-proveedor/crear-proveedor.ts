import { Component, Inject, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { ProveedorNoFormalizado } from '../../../../core/models/compras-no-formalizadas/proveedores/proveedor-no-formalizado.model';
import { ProveedorNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/proveedores/proveedor-no-formalizado.service';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-crear-proveedor',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule],
  templateUrl: './crear-proveedor.html',
  styleUrl: './crear-proveedor.scss',
})
export class CrearProveedor implements OnInit {

  proveedor: ProveedorNoFormalizado = {
    nombre: '',
    documento: '',
    contacto: '',
    telefono: '',
    correoElectronico: '',
    direccion: '',
    ciudad: '',
    departamento: ''
  };

  constructor(
    private proveedorService: ProveedorNoFormalizadoService,
    private dialogRef: MatDialogRef<ProveedorNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public data: ProveedorNoFormalizado | null,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    if (this.data) {
      this.proveedor = { ...this.data };
    }
  }

  cerrar(): void {
    this.dialogRef.close();
  }

  guardar(): void {
    if (this.proveedor.idProveedorNoFormalizado) {
      this.proveedorService.actualizar(this.proveedor.idProveedorNoFormalizado, this.proveedor).subscribe({
        next: () => {
          this.dialogRef.close(true);
        },
        error: (error) => {
          this.toastr.error(error.error?.message || 'No se pudo actualizar el proveedor.', 'Error');
        }
      });
    } else {
      this.proveedorService.crear(this.proveedor).subscribe({
        next: (proveedor) => {
          this.dialogRef.close(proveedor);
        },
        error: (error) => {
          this.toastr.error(error.error?.message || 'No se pudo crear el proveedor.', 'Error');
        }
      });
    }
  }
}
