import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  FiltrosMovimientoProducto,
  MovimientoProductoService
} from '../../../core/services/HistorialVersionOdo/movimiento-producto.service';

import { MovimientoProductoModel } from '../../../core/models/HistorialVersionOdo/movimiento-producto.model';
import { EstadisticasMovimientoProducto } from '../../../core/models/HistorialVersionOdo/estadisticas-movimiento-producto.model';

@Component({
  selector: 'app-movimiento-producto',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './movimiento-producto.html',
  styleUrl: './movimiento-producto.scss',
})
export class MovimientoProducto implements OnInit {

  movimientos: MovimientoProductoModel[] = [];

  estadisticas: EstadisticasMovimientoProducto = {
    totalMovimientos: 0,
    movimientosActivos: 0,
    movimientosCancelados: 0,
    cantidadTotal: 0
  };

  pagina = 1;
  porPagina = 50;

  totalRegistros = 0;
  totalPaginas = 0;

  buscar = '';
  producto = '';
  proveedor = '';
  estado = '';
  unidadMedida = '';
  fechaDesde = '';
  fechaHasta = '';

  cargando = false;
  cargandoEstadisticas = false;
  error = '';

  constructor(
    private movimientoProductoService: MovimientoProductoService
  ) { }

  ngOnInit(): void {
    this.cargarMovimientos();
  }

  cargarMovimientos(): void {

    this.cargando = true;
    this.error = '';

    const filtros: FiltrosMovimientoProducto = {
      pagina: this.pagina,
      porPagina: this.porPagina,
      buscar: this.buscar,
      producto: this.producto,
      proveedor: this.proveedor,
      estado: this.estado,
      unidadMedida: this.unidadMedida,
      fechaDesde: this.fechaDesde,
      fechaHasta: this.fechaHasta
    };

    this.movimientoProductoService.obtener(filtros).subscribe({
      next: (respuesta) => {

        this.movimientos = respuesta.datos;
        this.pagina = respuesta.pagina;
        this.porPagina = respuesta.porPagina;
        this.totalRegistros = respuesta.totalRegistros;
        this.totalPaginas = respuesta.totalPaginas;

        this.cargando = false;
      },

      error: (error) => {

        console.error(error);

        this.error =
          'No fue posible cargar los movimientos de producto.';

        this.cargando = false;
      }
    });

    this.cargarEstadisticas();
  }

  cargarEstadisticas(): void {

    this.cargandoEstadisticas = true;

    const filtros: FiltrosMovimientoProducto = {
      buscar: this.buscar,
      producto: this.producto,
      proveedor: this.proveedor,
      estado: this.estado,
      unidadMedida: this.unidadMedida,
      fechaDesde: this.fechaDesde,
      fechaHasta: this.fechaHasta
    };

    this.movimientoProductoService
      .obtenerEstadisticas(filtros)
      .subscribe({
        next: (respuesta) => {

          this.estadisticas = respuesta;
          this.cargandoEstadisticas = false;
        },

        error: (error) => {

          console.error(error);

          this.cargandoEstadisticas = false;
        }
      });
  }

  buscarMovimientos(): void {

    this.pagina = 1;
    this.cargarMovimientos();
  }

  limpiarFiltros(): void {

    this.buscar = '';
    this.producto = '';
    this.proveedor = '';
    this.estado = '';
    this.unidadMedida = '';
    this.fechaDesde = '';
    this.fechaHasta = '';

    this.pagina = 1;

    this.cargarMovimientos();
  }

  cambiarPagina(pagina: number): void {

    if (
      pagina < 1 ||
      pagina > this.totalPaginas ||
      pagina === this.pagina
    ) {
      return;
    }

    this.pagina = pagina;
    this.cargarMovimientos();
  }

  cambiarPorPagina(): void {

    this.pagina = 1;
    this.cargarMovimientos();
  }

  obtenerPaginas(): number[] {

    const paginas: number[] = [];

    const inicio = Math.max(
      1,
      this.pagina - 2
    );

    const fin = Math.min(
      this.totalPaginas,
      this.pagina + 2
    );

    for (let i = inicio; i <= fin; i++) {
      paginas.push(i);
    }

    return paginas;
  }
}
