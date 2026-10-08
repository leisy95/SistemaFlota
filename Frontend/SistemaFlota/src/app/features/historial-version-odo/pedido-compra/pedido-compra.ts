import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import { PedidoCompraModel } from '../../../core/models/HistorialVersionOdo/pedido-compra.model';
import { EstadisticasPedidoCompra } from '../../../core/models/HistorialVersionOdo/estadisticas-pedido-compra.model';
import { FiltrosPedidoCompra, PedidoCompraService } from '../../../core/services/HistorialVersionOdo/pedido-compra.service';

@Component({
  selector: 'app-pedido-compra',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pedido-compra.html',
  styleUrl: './pedido-compra.scss',
})
export class PedidoCompra implements OnInit {

  pedidos: PedidoCompraModel[] = [];
  estadisticas: EstadisticasPedidoCompra = {
    totalPedidos: 0,
    pedidosActivos: 0,
    pedidosCancelados: 0,
    valorTotal: 0
  };

  pagina = 1;
  porPagina = 20;
  totalRegistros = 0;
  totalPaginas = 0;

  buscar = '';
  prioridad = '';
  estado = '';
  fechaDesde = '';
  fechaHasta = '';

  cargando = false;
  cargandoEstadisticas = false;
  error = '';

  constructor(private pedidoCompraService: PedidoCompraService) { }

  ngOnInit(): void {
    this.cargarPedidos();
  }

  cargarPedidos(): void {
    this.cargando = true;
    this.error = '';

    const filtros: FiltrosPedidoCompra = {
      pagina: this.pagina,
      porPagina: this.porPagina,
      buscar: this.buscar,
      prioridad: this.prioridad,
      estado: this.estado,
      fechaDesde: this.fechaDesde,
      fechaHasta: this.fechaHasta
    };

    this.pedidoCompraService.obtener(filtros).subscribe({
      next: (respuesta) => {
        this.pedidos = respuesta.datos;
        this.pagina = respuesta.pagina;
        this.porPagina = respuesta.porPagina;
        this.totalRegistros = respuesta.totalRegistros;
        this.totalPaginas = respuesta.totalPaginas;
        this.cargando = false;
      },
      error: (error) => {
        console.error(error);
        this.error = 'No fue posible cargar los pedidos de compra.';
        this.cargando = false;
      }
    });

    this.cargarEstadisticas();
  }

  cargarEstadisticas(): void {
    this.cargandoEstadisticas = true;

    const filtros: FiltrosPedidoCompra = {
      buscar: this.buscar,
      prioridad: this.prioridad,
      estado: this.estado,
      fechaDesde: this.fechaDesde,
      fechaHasta: this.fechaHasta
    };

    this.pedidoCompraService.obtenerEstadisticas(filtros).subscribe({
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

  buscarPedidos(): void {
    this.pagina = 1;
    this.cargarPedidos();
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.prioridad = '';
    this.estado = '';
    this.fechaDesde = '';
    this.fechaHasta = '';
    this.pagina = 1;
    this.cargarPedidos();
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas || pagina === this.pagina) {
      return;
    }

    this.pagina = pagina;
    this.cargarPedidos();
  }

  cambiarPorPagina(): void {
    this.pagina = 1;
    this.cargarPedidos();
  }

  obtenerPaginas(): number[] {
    const paginas: number[] = [];
    const inicio = Math.max(1, this.pagina - 2);
    const fin = Math.min(this.totalPaginas, this.pagina + 2);

    for (let i = inicio; i <= fin; i++) {
      paginas.push(i);
    }

    return paginas;
  }

  formatearTotal(total: number): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      maximumFractionDigits: 0
    }).format(total);
  }
}
