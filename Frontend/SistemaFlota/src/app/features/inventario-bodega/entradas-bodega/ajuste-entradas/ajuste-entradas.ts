
import { CommonModule, Location } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';
import { MostrarEntradaDto } from '../../../../core/models/inventario-bodega/entrada-bodega/entradas-bodega.models';

@Component({
  selector: 'app-ajuste-entradas',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './ajuste-entradas.html',
  styleUrl: './ajuste-entradas.scss',
})
export class AjusteEntradas implements OnInit {

  items: MostrarEntradaDto[] = [];
  loading = false;
  ajusteForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private entradaService: EntradaInventarioService,
    private toastr: ToastrService,
    private router: Router,
    private location: Location
  ) {
    this.ajusteForm = this.fb.group({
      observacion: ['', [
        Validators.required,
        Validators.maxLength(500)
      ]]
    });
  }

  ngOnInit(): void {
    this.obtenerItemsSeleccionados();
  }
  // Recupera los elementos enviados desde la pantalla anterior.
  private obtenerItemsSeleccionados(): void {
    const state = this.location.getState() as {
      items?: MostrarEntradaDto[];
    };

    this.items = state?.items ?? [];

    if (this.items.length === 0) {
      this.toastr.warning(
        'No hay elementos seleccionados para realizar el ajuste.'
      );
      this.router.navigate(['/inventario-bodega/mostrar-entradas']);
      return;
    }

    // Solo se permiten elementos que estén disponibles en almacén.
    this.items = this.items.filter(
      item => item.estado?.toUpperCase() === 'EN_ALMACEN'
    );

    if (this.items.length === 0) {
      this.toastr.warning(
        'Los elementos seleccionados no están disponibles para ajuste.'
      );
      this.router.navigate(['/inventario-bodega/mostrar-entradas']);
    }
  }

  confirmarAjuste(): void {
    if (this.ajusteForm.invalid) {
      this.ajusteForm.markAllAsTouched();
      return;
    }

    if (this.items.length === 0) {
      this.toastr.warning('No hay elementos para ajustar.');
      return;
    }

    const observacion = this.ajusteForm.value.observacion?.trim();

    if (!observacion) {
      this.toastr.warning('Ingrese una observación para el ajuste.');
      return;
    }

    const confirmar = window.confirm(
      `¿Está seguro de realizar el ajuste de ${this.items.length} elementos?`
    );

    if (!confirmar) return;

    const request = {
      itemIds: this.items.map(item => item.id),
      observacion
    };

    this.loading = true;

    this.entradaService.ajusteGeneral(request).subscribe({
      next: (respuesta) => {
        this.toastr.success(
          respuesta.message || 'Ajuste realizado correctamente.'
        );

        this.ajusteForm.reset();
        this.items = [];

        this.router.navigate(['/inventario-bodega/mostrar-entradas']);
      },
      error: (error) => {
        console.error('Error al realizar el ajuste:', error);

        this.toastr.error(
          error.error?.message || 'No fue posible realizar el ajuste.'
        );
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  cancelar(): void {
    this.router.navigate(['/inventario-bodega/mostrar-entradas']);
  }
}
