import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CrearProveedor } from '../../proveedores/crear-proveedor/crear-proveedor';
@Component({
  selector: 'app-crear-materiales',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './crear-materiales.html',
  styleUrl: './crear-materiales.scss',
})
export class CrearMateriales {

  constructor(
    private dialogRef: MatDialogRef<CrearProveedor>,
    @Inject(MAT_DIALOG_DATA) public data: any

  ) { }

  material = {
    proveedor: '',
    materiaPrima: '',
    densidad: 'PEBD',
    calidad: 'Original',
    color: '',
    lineaProduccion: '',
    precioBase: 0,
    bultos: 0,
    cantidad: 0,
    estado: 'Activo'
  };

  proveedores = [
    { id: 1, nombre: 'Plásticos del Norte S.A.' },
    { id: 2, nombre: 'Resinas Premium Ltda.' },
    { id: 3, nombre: 'Gecobags S.A.S.' }
  ];

  densidades = [
    'PEBD',
    'PEAD',
    'PP',
    'PVC'
  ];

  calidades = [
    'Original',
    'Post-industrial',
    'Reciclado'
  ];

  estados = [
    'Activo',
    'Inactivo'
  ];

  guardar(): void {
    console.log('Material a guardar:', this.material);

    // Aquí luego llamarás al servicio
    // this.materialService.crear(this.material).subscribe(...)
  }

  cerrar(): void {
    this.dialogRef.close();
  }

}