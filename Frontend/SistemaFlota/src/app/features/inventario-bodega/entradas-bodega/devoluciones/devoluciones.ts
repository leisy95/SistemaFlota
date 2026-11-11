
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';

interface InventarioItem {
  id: number;
  referencia: string;
  referenciaPeso: string;
  fechaRegistroItem: string;
  pesoActual: number;
  estado: string;
}

@Component({
  selector: 'app-devoluciones',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './devoluciones.html',
  styleUrl: './devoluciones.scss',
})
export class Devoluciones {

  form: FormGroup;

  cargando = false;
  buscando = false;

  inventarioItemId: number | null = null;

  dataSource: InventarioItem[] = [];

  constructor(
    private fb: FormBuilder,
    private toastr: ToastrService,
    private router: Router,
    private entradaService: EntradaInventarioService
  ) {
    this.form = this.fb.group({
      referenciaPeso: ['', Validators.required],
      observacion: ['', Validators.required]
    });
  }

  // BUSCAR PRODUCTO POR CÓDIGO DE BARRAS
  buscarItem(): void {

    const codigo = this.form.get('referenciaPeso')?.value?.trim();

    if (!codigo) {
      this.form.get('referenciaPeso')?.markAsTouched();

      this.toastr.warning(
        'Ingrese o escanee un código de barras.'
      );

      return;
    }

    this.buscando = true;

    this.inventarioItemId = null;
    this.dataSource = [];

    // CONSULTAR INVENTARIO REAL
    this.entradaService.obtenerEntradas().subscribe({

      next: (respuesta) => {

        const itemEncontrado = (respuesta ?? []).find(
          item =>
            item.referenciaPeso?.trim().toLowerCase() ===
            codigo.toLowerCase()
        );

        if (!itemEncontrado) {

          this.toastr.error(
            'No se encontró ningún producto con ese código.'
          );

          this.buscando = false;
          return;
        }

        // VALIDAR QUE EL ITEM ESTÉ EN ESTADO SALIDA
        if (itemEncontrado.estado?.toUpperCase() !== 'SALIDA') {

          this.toastr.warning(
            `El item no puede devolverse porque su estado actual es ${itemEncontrado.estado}.`
          );

          this.buscando = false;
          return;
        }

        this.inventarioItemId = itemEncontrado.id;

        this.dataSource = [itemEncontrado];

        this.toastr.success(
          'Producto encontrado y disponible para devolución.'
        );

        this.buscando = false;

      },

      error: (error) => {

        console.error('Error al buscar item:', error);

        this.toastr.error(
          'No fue posible consultar el inventario.'
        );

        this.buscando = false;

      }

    });
  }

  // REGISTRAR DEVOLUCIÓN REAL
  confirmarDevolucion(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.toastr.warning(
        'Complete todos los campos obligatorios.'
      );

      return;
    }

    if (!this.inventarioItemId || this.dataSource.length === 0) {

      this.toastr.warning(
        'Primero debe buscar un producto válido.'
      );

      return;
    }

    const confirmar = confirm(
      '¿Está seguro de registrar la devolución de este producto al almacén?'
    );

    if (!confirmar) return;

    this.cargando = true;

    const request = {
      inventarioBodegaItemId: this.inventarioItemId,
      observacion: this.form.get('observacion')?.value.trim()
    };

    this.entradaService.registrarDevolucion(request).subscribe({

      next: (respuesta) => {

        this.toastr.success(
          respuesta.message || 'Devolución registrada correctamente.'
        );

        this.limpiarFormulario();

        this.cargando = false;

      },

      error: (error) => {

        console.error('Error al registrar devolución:', error);

        const mensaje =
          error.error?.message ||
          'No fue posible registrar la devolución.';

        this.toastr.error(mensaje);

        this.cargando = false;

      }

    });
  }

  // REGRESAR
  regresar(): void {

    this.limpiarFormulario();

    this.router.navigate([
      '/inventario-bodega/mostrar-entradas'
    ]);

  }

  // LIMPIAR FORMULARIO
  private limpiarFormulario(): void {

    this.form.reset({
      referenciaPeso: '',
      observacion: ''
    });

    this.dataSource = [];

    this.inventarioItemId = null;

    this.cargando = false;

  }

}