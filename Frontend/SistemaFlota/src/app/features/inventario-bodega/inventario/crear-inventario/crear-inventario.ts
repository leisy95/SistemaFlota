
import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';

@Component({
  selector: 'app-crear-inventario',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './crear-inventario.html',
  styleUrl: './crear-inventario.scss',
})
export class CrearInventario {

  form: FormGroup;

  esEdicion = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CrearInventario>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {

    this.esEdicion = !!data;

    this.form = this.fb.group({

      referencia: ['', Validators.required],
      ancho: [null],
      alto: [null],
      calibre: [null],
      fuelleLateral: [null],
      fuelleFondo: [null],
      fuelleSuperior: [null],
      tipoBolsa: [''],
      tipoMaterial: [''],
      densidad: [''],
      color: [''],
      segundoColor: [''],
      impresoNo: [''],
      troquel: [''],
      referenciaImpresa: [''],
      paqueteNumero: [null],
      paqueteCantidad: [null],

      // Existencias: solo lectura en creación y edición
      peso: [{ value: 0, disabled: true }],
      cantidad: [{ value: 0, disabled: true }]

    });

    // Cargar información del producto al editar
    if (this.data) {
      this.form.patchValue(this.data);
    }
  }

  guardar(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // Los campos deshabilitados no se incluyen en form.value
    const producto = this.form.value;

    this.dialogRef.close(producto);
  }

  cancelar(): void {
    this.dialogRef.close();
  }
}
