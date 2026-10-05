import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AjusteInventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/ajuste-inventario-no-formalizado.service';
import { ToastrService } from 'ngx-toastr';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-ajuste-inventario-no-formalizado',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule],
  templateUrl: './ajuste-inventario-no-formalizado.html',
  styleUrl: './ajuste-inventario-no-formalizado.scss',
})
export class AjusteInventarioNoFormalizado implements OnInit {
  inventario: any;
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private service: AjusteInventarioNoFormalizadoService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<AjusteInventarioNoFormalizado>,
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
        this.toastr.error('No fue posible cargar el inventario no formalizado.');
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

    this.service.crear(dto as any).subscribe({
      next: () => {
        this.toastr.success('Ajuste de inventario no formalizado realizado correctamente.');
        this.dialogRef.close(true);
      },
      error: err => {
        const mensaje = err.error?.mensaje || err.error || 'No fue posible realizar el ajuste.';
        this.toastr.error(mensaje);
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
