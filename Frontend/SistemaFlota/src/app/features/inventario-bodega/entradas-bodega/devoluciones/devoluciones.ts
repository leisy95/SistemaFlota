
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';
import { Dialog } from '../../../../shared/reutilizable/dialog/dialog';

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
    private router: Router,
    private dialog: MatDialog,
    private entradaService: EntradaInventarioService
  ) {
    this.form = this.fb.group({
      referenciaPeso: ['', Validators.required],
      observacion: ['', Validators.required]
    });
  }

  // MOSTRAR ALERTAS PERSONALIZADAS
  private mostrarDialogo(
    titulo: string,
    mensaje: string,
    tipo: string = 'info'
  ): void {
    this.dialog.open(Dialog, {
      width: '400px',
      disableClose: true,
      data: {
        titulo,
        mensaje,
        tipo,
        textoBoton: 'Aceptar',
        mostrarCancelar: false
      }
    });
  }

  // BUSCAR PRODUCTO POR CÓDIGO DE BARRAS
  buscarItem(): void {

    const codigo = this.form.get('referenciaPeso')?.value?.trim();

    if (!codigo) {
      this.form.get('referenciaPeso')?.markAsTouched();

      this.mostrarDialogo(
        'Código requerido',
        'Ingrese o escanee un código de barras.',
        'warning'
      );

      return;
    }

    this.buscando = true;
    this.inventarioItemId = null;
    this.dataSource = [];

    this.entradaService.obtenerEntradas('SALIDA').subscribe({

      next: (respuesta) => {

        const itemEncontrado = (respuesta ?? []).find(
          item =>
            item.referenciaPeso?.trim().toLowerCase() ===
            codigo.toLowerCase()
        );

        if (!itemEncontrado) {

          this.mostrarDialogo(
            'Producto no encontrado',
            'No se encontró ningún producto con ese código.',
            'error'
          );

          this.buscando = false;
          return;
        }

        if (itemEncontrado.estado?.toUpperCase() !== 'SALIDA') {

          this.mostrarDialogo(
            'Estado no válido',
            `El item no puede devolverse porque su estado actual es ${itemEncontrado.estado}.`,
            'warning'
          );

          this.buscando = false;
          return;
        }

        this.inventarioItemId = itemEncontrado.id;
        this.dataSource = [itemEncontrado];

        this.mostrarDialogo(
          'Producto encontrado',
          'Producto encontrado y disponible para devolución.',
          'success'
        );

        this.buscando = false;

      },

      error: (error) => {

        console.error('Error al buscar item:', error);

        this.mostrarDialogo(
          'Error de consulta',
          'No fue posible consultar el inventario.',
          'error'
        );

        this.buscando = false;

      }

    });
  }

  // REGISTRAR DEVOLUCIÓN REAL
  confirmarDevolucion(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.mostrarDialogo(
        'Campos obligatorios',
        'Complete todos los campos obligatorios.',
        'warning'
      );

      return;
    }

    if (!this.inventarioItemId || this.dataSource.length === 0) {

      this.mostrarDialogo(
        'Producto requerido',
        'Primero debe buscar un producto válido.',
        'warning'
      );

      return;
    }

    // CONFIRMACIÓN CON MATDIALOG
    const dialogRef = this.dialog.open(Dialog, {
      width: '400px',
      disableClose: true,
      data: {
        titulo: 'Confirmar devolución',
        mensaje: '¿Está seguro de registrar la devolución de este producto al almacén?',
        tipo: 'question',
        textoBoton: 'Sí, devolver',
        textoCancelar: 'Cancelar',
        mostrarCancelar: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmado => {

      if (!confirmado) return;

      this.cargando = true;

      const request = {
        inventarioBodegaItemId: this.inventarioItemId!,
        observacion: this.form.get('observacion')?.value.trim()
      };

      this.entradaService.registrarDevolucion(request).subscribe({

        next: (respuesta) => {

          this.mostrarDialogo(
            'Devolución registrada',
            respuesta.message || 'Devolución registrada correctamente.',
            'success'
          );

          this.limpiarFormulario();

        },

        error: (error) => {

          console.error('Error al registrar devolución:', error);

          const mensaje =
            error.error?.message ||
            'No fue posible registrar la devolución.';

          this.mostrarDialogo(
            'Error al devolver',
            mensaje,
            'error'
          );

          this.cargando = false;

        }

      });

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
