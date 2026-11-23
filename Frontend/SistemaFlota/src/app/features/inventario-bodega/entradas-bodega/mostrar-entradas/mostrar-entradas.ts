import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';
import { NgxBarcode6 } from 'ngx-barcode6';

import {
  MostrarEntradaDto
} from '../../../../core/models/inventario-bodega/entrada-bodega/entradas-bodega.models';

import { InventarioBodega } from '../../../../core/models/inventario-bodega/inventario-bodega.models';
import { InventarioBodegaService } from '../../../../core/services/inventario-bodega/Inventario/inventario-bodega.service';

type EntradaInventario = MostrarEntradaDto;

@Component({
  selector: 'app-mostrar-entradas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    NgxBarcode6
  ],
  templateUrl: './mostrar-entradas.html',
  styleUrl: './mostrar-entradas.scss',
})
export class MostrarEntradas implements OnInit {

  // DATOS PARA REIMPRESIÓN
  itemParaImprimir: EntradaInventario | null = null;
  inventarioEtiqueta: InventarioBodega | null = null;

  barcodeValue = '';
  fechaReimpresion = new Date();

  rol = 'Administrador';

  // AJUSTE GENERAL
  modoAjuste = false;
  seleccionadas: number[] = [];
  private entradasSeleccionadas: EntradaInventario[] = [];

  // FILTROS
  buscar = '';
  estado = 'TODOS';
  fechaInicio = '';
  fechaFin = '';

  // PAGINACIÓN
  paginaActual = 1;
  pageSize = 5;
  totalRegistros = 0;
  totalPaginas = 1;

  // DATOS
  entradas: EntradaInventario[] = [];

  cargando = false;
  cargandoAjuste = false;

  constructor(
    private entradaService: EntradaInventarioService,
    private inventarioService: InventarioBodegaService,
    private toastr: ToastrService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.cargarEntradas();
  }

  cargarEntradas(): void {
    this.cargando = true;
    this.entradaService.filtrarEntradas(this.buscar, 'EN_ALMACEN', this.fechaInicio || undefined, this.fechaFin || undefined, this.paginaActual, this.pageSize).subscribe({
      next: (respuesta) => {
        this.entradas = respuesta?.items ?? [];
        this.totalRegistros = respuesta?.total ?? 0;
        this.totalPaginas = Math.max(1, respuesta?.totalPaginas ?? 1);
        this.paginaActual = respuesta?.pagina ?? this.paginaActual;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al consultar entradas:', error);
        this.entradas = [];
        this.totalRegistros = 0;
        this.totalPaginas = 1;
        this.toastr.error('No fue posible cargar las entradas de inventario.');
        this.cargando = false;
      }
    });
  }

  aplicarFiltro(event: Event): void {

    this.buscar = (
      event.target as HTMLInputElement
    ).value;

    this.paginaActual = 1;
    this.cargarEntradas();

  }

  filtrarPorEstado(event: Event): void {

    this.estado = (
      event.target as HTMLSelectElement
    ).value;

    this.paginaActual = 1;
    this.cargarEntradas();

  }

  filtrarPorFechaInicio(event: Event): void {

    this.fechaInicio = (
      event.target as HTMLInputElement
    ).value;

    this.paginaActual = 1;
    this.cargarEntradas();

  }

  filtrarPorFechaFin(event: Event): void {

    this.fechaFin = (
      event.target as HTMLInputElement
    ).value;

    this.paginaActual = 1;
    this.cargarEntradas();

  }

  limpiarFiltros(): void {

    this.buscar = '';
    this.estado = 'TODOS';
    this.fechaInicio = '';
    this.fechaFin = '';

    this.paginaActual = 1;

    this.cargarEntradas();

  }

  get total(): number {
    return this.totalRegistros;
  }

  get totalEnAlmacen(): number {
    return this.entradas.filter(
      item => item.estado === 'EN_ALMACEN'
    ).length;
  }

  get totalOtrosEstados(): number {
    return this.entradas.filter(
      item => item.estado !== 'EN_ALMACEN'
    ).length;
  }

  get registrosPaginados(): EntradaInventario[] {
    return this.entradas;
  }

  get registrosInicio(): number {

    return this.totalRegistros === 0
      ? 0
      : (this.paginaActual - 1) * this.pageSize + 1;

  }

  get registrosFin(): number {

    return Math.min(
      this.paginaActual * this.pageSize,
      this.totalRegistros
    );

  }

  cambiarPageSize(event: Event): void {

    this.pageSize = Number(
      (event.target as HTMLSelectElement).value
    );

    this.paginaActual = 1;
    this.cargarEntradas();

  }

  paginaAnterior(): void {

    if (this.paginaActual > 1 && !this.cargando) {

      this.paginaActual--;
      this.cargarEntradas();

    }

  }

  paginaSiguiente(): void {

    if (
      this.paginaActual < this.totalPaginas &&
      !this.cargando
    ) {

      this.paginaActual++;
      this.cargarEntradas();

    }

  }

  activarModoAjuste(): void {

    if (this.rol !== 'Administrador') {
      return;
    }

    this.modoAjuste = true;
    this.seleccionadas = [];
    this.entradasSeleccionadas = [];

  }

  cancelarAjuste(): void {

    this.modoAjuste = false;
    this.seleccionadas = [];
    this.entradasSeleccionadas = [];

  }

  // SELECCIONAR SOLAMENTE ITEMS EN ALMACÉN
  toggleSeleccion(item: EntradaInventario): void {

    if (item.estado !== 'EN_ALMACEN') {

      this.toastr.warning(
        'Solo puedes seleccionar items que estén en almacén.'
      );

      return;
    }

    if (this.estaSeleccionada(item)) {

      this.seleccionadas = this.seleccionadas.filter(
        id => id !== item.id
      );

      this.entradasSeleccionadas =
        this.entradasSeleccionadas.filter(
          seleccionado => seleccionado.id !== item.id
        );

    } else {

      this.seleccionadas.push(item.id);
      this.entradasSeleccionadas.push(item);

    }

  }

  estaSeleccionada(item: EntradaInventario): boolean {

    return this.seleccionadas.includes(item.id);

  }

  confirmarAjusteGeneral(): void {

    if (this.rol !== 'Administrador') {

      this.toastr.warning(
        'No tienes permisos para realizar ajustes.'
      );

      return;
    }

    if (this.seleccionadas.length === 0) {

      this.toastr.warning(
        'Debes seleccionar al menos un item.'
      );

      return;
    }

    const itemsSeleccionados = this.entradasSeleccionadas.filter(
      item => item.estado?.toUpperCase() === 'EN_ALMACEN'
    );

    if (itemsSeleccionados.length === 0) {

      this.toastr.warning(
        'No hay elementos disponibles para ajustar.'
      );

      return;
    }

    this.router.navigate(
      ['/inventario-bodega/ajuste-general'],
      {
        state: {
          items: itemsSeleccionados
        }
      }
    );

  }

  eliminarItem(id: number): void {

    const confirmar = confirm(
      '¿Estás seguro de eliminar esta entrada?'
    );

    if (!confirmar) return;

    // Pendiente de conectar con el endpoint de eliminación.
    console.log('Eliminar entrada:', id);

  }

  reimprimirEtiqueta(item: EntradaInventario): void {

    this.inventarioService.obtenerPorReferencia(
      item.referencia
    ).subscribe({

      next: (resultado) => {

        this.itemParaImprimir = item;
        this.inventarioEtiqueta = resultado;

        this.barcodeValue = item.referenciaPeso;
        this.fechaReimpresion = new Date(
          item.fechaRegistroItem
        );

        setTimeout(() => {
          window.print();
        }, 300);

      },

      error: (error) => {

        console.error(
          'Error al consultar referencia:',
          error
        );

        this.toastr.error(
          'No fue posible consultar la información de la etiqueta.'
        );

      }

    });

  }

  exportarPdf(): void {

    this.entradaService.exportarPdf(
      this.buscar,
      this.fechaInicio || undefined,
      this.fechaFin || undefined
    ).subscribe({

      next: (pdf) => {

        const url = window.URL.createObjectURL(pdf);
        const enlace = document.createElement('a');

        enlace.href = url;
        enlace.download =
          `Entradas_Inventario_${new Date().getTime()}.pdf`;

        enlace.click();

        window.URL.revokeObjectURL(url);

        this.toastr.success(
          'PDF generado correctamente.'
        );

      },

      error: (error) => {

        console.error(
          'Error al generar PDF:',
          error
        );

        this.toastr.error(
          'No fue posible generar el PDF.'
        );

      }

    });

  }

}