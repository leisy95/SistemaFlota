
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';
import { NgxBarcode6 } from 'ngx-barcode6';

interface EntradaInventario {
  id: number;
  referencia: string;
  referenciaPeso: string;
  fechaRegistroItem: string;
  pesoActual: number;
  estado: string;
}

@Component({
  selector: 'app-mostrar-entradas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './mostrar-entradas.html',
  styleUrl: './mostrar-entradas.scss',
})
export class MostrarEntradas implements OnInit {

  rol = 'Administrador';

  modoAjuste = false;

  buscar = '';
  fechaFiltro = '';

  observacionAjuste = '';

  paginaActual = 1;
  pageSize = 5;

  seleccionadas: number[] = [];

  entradas: EntradaInventario[] = [];

  // PARA REIMPRESIÓN
  itemParaImprimir: EntradaInventario | null = null;
  fechaReimpresion = new Date();

  cargando = false;
  cargandoAjuste = false;

  constructor(
    private entradaService: EntradaInventarioService,
    private toastr: ToastrService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.cargarEntradas();
  }

  // CONSULTAR ENTRADAS DESDE LA API
  cargarEntradas(): void {

    this.cargando = true;

    this.entradaService.obtenerEntradas().subscribe({

      next: (respuesta) => {

        this.entradas = respuesta ?? [];

        this.paginaActual = 1;
        this.cargando = false;

      },

      error: (error) => {

        console.error('Error al consultar entradas:', error);

        this.toastr.error(
          'No fue posible cargar las entradas de inventario.'
        );

        this.cargando = false;

      }

    });
  }

  // FILTROS
  get entradasFiltradas(): EntradaInventario[] {

    return this.entradas.filter(item => {

      const texto = this.buscar.trim().toLowerCase();

      const coincideTexto =
        (item.referencia ?? '').toLowerCase().includes(texto) ||
        (item.referenciaPeso ?? '').toLowerCase().includes(texto) ||
        (item.estado ?? '').toLowerCase().includes(texto);

      const fechaItem = item.fechaRegistroItem
        ? this.obtenerFechaLocal(item.fechaRegistroItem)
        : '';

      const coincideFecha =
        !this.fechaFiltro || fechaItem === this.fechaFiltro;

      return coincideTexto && coincideFecha;

    });
  }

  private obtenerFechaLocal(fecha: string): string {

    const date = new Date(fecha);

    if (isNaN(date.getTime())) {
      return '';
    }

    const anio = date.getFullYear();
    const mes = String(date.getMonth() + 1).padStart(2, '0');
    const dia = String(date.getDate()).padStart(2, '0');

    return `${anio}-${mes}-${dia}`;
  }

  // ESTADÍSTICAS
  get total(): number {
    return this.entradas.length;
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

  // PAGINACIÓN
  get totalPaginas(): number {
    return Math.max(
      1,
      Math.ceil(this.entradasFiltradas.length / this.pageSize)
    );
  }

  get registrosPaginados(): EntradaInventario[] {

    const inicio = (this.paginaActual - 1) * this.pageSize;

    return this.entradasFiltradas.slice(
      inicio,
      inicio + this.pageSize
    );
  }

  get registrosInicio(): number {
    return this.entradasFiltradas.length === 0
      ? 0
      : (this.paginaActual - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {
    return Math.min(
      this.paginaActual * this.pageSize,
      this.entradasFiltradas.length
    );
  }

  // EVENTOS DE FILTROS
  aplicarFiltro(event: Event): void {

    this.buscar = (
      event.target as HTMLInputElement
    ).value;

    this.paginaActual = 1;
  }

  filtrarPorFecha(event: Event): void {

    this.fechaFiltro = (
      event.target as HTMLInputElement
    ).value;

    this.paginaActual = 1;
  }

  limpiarFiltros(): void {

    this.buscar = '';
    this.fechaFiltro = '';
    this.paginaActual = 1;

  }

  cambiarPageSize(event: Event): void {

    this.pageSize = Number(
      (event.target as HTMLSelectElement).value
    );

    this.paginaActual = 1;
  }

  paginaAnterior(): void {

    if (this.paginaActual > 1) {
      this.paginaActual--;
    }

  }

  paginaSiguiente(): void {

    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
    }

  }

  // AJUSTE GENERAL
  activarModoAjuste(): void {

    if (this.rol !== 'Administrador') {
      return;
    }

    this.modoAjuste = true;
    this.seleccionadas = [];
    this.observacionAjuste = '';

  }

  cancelarAjuste(): void {

    this.modoAjuste = false;
    this.seleccionadas = [];
    this.observacionAjuste = '';

  }

  // SOLO SE PUEDEN SELECCIONAR ITEMS DISPONIBLES
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

    } else {

      this.seleccionadas.push(item.id);

    }

  }

  estaSeleccionada(item: EntradaInventario): boolean {

    return this.seleccionadas.includes(item.id);

  }

  // EJECUTAR AJUSTE GENERAL REAL
  confirmarAjusteGeneral(): void {

    if (this.rol !== 'Administrador') {
      this.toastr.warning('No tienes permisos para realizar ajustes.');
      return;
    }

    if (this.seleccionadas.length === 0) {
      this.toastr.warning('Debes seleccionar al menos un item.');
      return;
    }

    const itemsSeleccionados = this.entradas.filter(
      item => this.seleccionadas.includes(item.id) &&
        item.estado?.toUpperCase() === 'EN_ALMACEN'
    );

    if (itemsSeleccionados.length === 0) {
      this.toastr.warning('No hay elementos disponibles para ajustar.');
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

  // ELIMINAR ENTRADA
  eliminarItem(id: number): void {

    const confirmar = confirm(
      '¿Estás seguro de eliminar esta entrada?'
    );

    if (!confirmar) return;

    // Pendiente de conectar con el endpoint de eliminación.
    console.log('Eliminar entrada:', id);

  }

  // REIMPRIMIR ETIQUETA
  reimprimirEtiqueta(item: EntradaInventario): void {

    // Pendiente de conectar con el endpoint de impresión.
    console.log('Reimprimir etiqueta:', item);

  }

}
