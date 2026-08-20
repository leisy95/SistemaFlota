import { Component, Inject } from '@angular/core';
import { AjusteInventarioService } from '../../../../core/services/costos/inventario/ajusteinventario.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-ajuste-inventario',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './ajuste-inventario.html',
  styleUrl: './ajuste-inventario.scss',
})
export class AjusteInventario {

  inventario: any;
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: AjusteInventarioService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<AjusteInventario>,
    @Inject(MAT_DIALOG_DATA) public inventarioId: number
  ) { }

  ngOnInit(): void {

    this.form = this.fb.group({
      tipo: ['', Validators.required],
      cantidad: [null, [Validators.required, Validators.min(0.01)]],
      motivo: ['', Validators.required],
      observaciones: ['']
    });

    this.cargarInventario();
  }

  cargarInventario(): void {
    this.service.obtenerInventario(this.inventarioId).subscribe({
      next: resp => this.inventario = resp,
      error: () => {
        this.toastr.error('No fue posible cargar el inventario.');
        this.dialogRef.close();
      }
    });
  }

  guardar(): void {

    if (this.form.invalid) return;

    const dto = {
      inventarioId: this.inventarioId,
      ...this.form.value
    };

    console.log(dto);

    this.service.crear(dto as any).subscribe({
      next: () => {
        this.toastr.success('Ajuste realizado correctamente.');
        this.dialogRef.close(true);
      },
      error: err => this.toastr.error(err.error)
    });

  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
