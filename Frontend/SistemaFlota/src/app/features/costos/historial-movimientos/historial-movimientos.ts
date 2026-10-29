
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  HistorialMovimientosService,
  FiltrosHistorialMovimientos
} from '../../../core/services/costos/historialMovimiento/historial-movimientos.service';

import {
  MovimientoHistorialResumen
} from '../../../core/models/costos/historialMovimiento/movimiento-historial-resumen.model';

import {
  MovimientoHistorial
} from '../../../core/models/costos/historialMovimiento/movimiento-historial.model';
import { MatDialog } from '@angular/material/dialog';
import { DetalleMovimiento } from './detalle-movimiento/detalle-movimiento';

interface MovimientoVista {
  id: number;
  fecha: Date;
  tipo: string;
  accion: string;
  descripcion: string;
  usuario: string;
  rol: string;
  ipAddress: string;
  referencia: string;
  detalles: string;
  resultado: string;
}

@Component({
  selector: 'app-historial-movimientos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './historial-movimientos.html',
  styleUrl: './historial-movimientos.scss',
})
export class HistorialMovimientos implements OnInit {

  private readonly historialService = inject(HistorialMovimientosService);
  private readonly dialog = inject(MatDialog);

  buscar = '';
  tipoSeleccionado = '';
  usuarioSeleccionado = '';

  movimientos: MovimientoVista[] = [];

  tipos: string[] = [];
  usuarios: string[] = [];

  resumen: MovimientoHistorialResumen = {
    totalMovimientos: 0,
    totalCreaciones: 0,
    totalActualizaciones: 0,
    totalEliminaciones: 0
  };

  cargando = false;
  error = '';

  // Paginación desde el backend.
  pagina = 1;
  porPagina = 10;
  totalRegistros = 0;
  totalPaginas = 0;

  ngOnInit(): void {
    this.cargarOpciones();
    this.cargarDatos();
  }

  /**
   * Carga todos los módulos y usuarios registrados,
   * independientemente de la paginación del historial.
   */
  private cargarOpciones(): void {
    this.historialService.obtenerOpciones().subscribe({
      next: opciones => {
        this.tipos = opciones.modulos ?? [];
        this.usuarios = opciones.usuarios ?? [];
      },
      error: err => {
        console.error('Error al cargar las opciones de filtros:', err);
      }
    });
  }

  /**
   * Envía los filtros al backend.
   * El filtrado se realiza antes de la paginación.
   */
  private obtenerFiltros(): FiltrosHistorialMovimientos {
    return {
      buscar: this.buscar.trim() || undefined,
      modulo: this.tipoSeleccionado || undefined,
      usuario: this.usuarioSeleccionado || undefined,
      pagina: this.pagina,
      porPagina: this.porPagina
    };
  }

  cargarDatos(): void {
    this.cargando = true;
    this.error = '';

    const filtros = this.obtenerFiltros();

    this.historialService.obtener(filtros).subscribe({
      next: respuesta => {
        this.movimientos = respuesta.movimientos.map(
          movimiento => this.mapearMovimiento(movimiento)
        );

        this.totalRegistros = respuesta.totalRegistros;
        this.totalPaginas = respuesta.totalPaginas;
        this.pagina = respuesta.pagina;
        this.cargando = false;
      },
      error: err => {
        console.error('Error al cargar el historial:', err);
        this.error = 'No fue posible cargar el historial de movimientos.';
        this.cargando = false;
      }
    });

    // Actualiza el resumen con los mismos filtros.
    this.historialService.obtenerResumen(filtros).subscribe({
      next: respuesta => {
        this.resumen = respuesta;
      },
      error: err => {
        console.error('Error al cargar el resumen:', err);
      }
    });
  }

  aplicarFiltros(): void {
    this.pagina = 1;
    this.cargarDatos();
  }

  private mapearMovimiento(
    movimiento: MovimientoHistorial
  ): MovimientoVista {
    return {
      id: movimiento.id,
      fecha: new Date(movimiento.fecha),
      tipo: movimiento.modulo ?? '',
      accion: movimiento.accion ?? '',
      descripcion: movimiento.detalle ?? '',
      usuario: movimiento.usuario ?? '',
      rol: movimiento.rol ?? '',
      ipAddress: movimiento.ipAddress ?? '',
      referencia: movimiento.registroId != null
        ? `ID-${movimiento.registroId}`
        : `AUD-${movimiento.id}`,
      detalles: movimiento.detalle ?? '',
      resultado: movimiento.resultado ?? ''
    };
  }

  get totalMovimientos(): number {
    return this.resumen.totalMovimientos;
  }

  get totalCreaciones(): number {
    return this.resumen.totalCreaciones;
  }

  get totalActualizaciones(): number {
    return this.resumen.totalActualizaciones;
  }

  get totalEliminaciones(): number {
    return this.resumen.totalEliminaciones;
  }

  /**
   * El backend ya devuelve los resultados filtrados.
   */
  get movimientosFiltrados(): MovimientoVista[] {
    return this.movimientos;
  }

  cambiarPagina(nuevaPagina: number): void {
    if (
      nuevaPagina < 1 ||
      nuevaPagina > this.totalPaginas ||
      nuevaPagina === this.pagina ||
      this.cargando
    ) {
      return;
    }

    this.pagina = nuevaPagina;
    this.cargarDatos();
  }

  cambiarPorPagina(valor: number | string): void {
    const cantidad = Number(valor);

    if (![10, 25, 50, 100].includes(cantidad)) {
      return;
    }

    this.porPagina = cantidad;
    this.pagina = 1;
    this.cargarDatos();
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.tipoSeleccionado = '';
    this.usuarioSeleccionado = '';

    this.aplicarFiltros();
  }

  verDetalles(movimiento: MovimientoVista): void {
    this.dialog.open(DetalleMovimiento, {
      width: '650px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      autoFocus: false,
      data: movimiento
    });
  }
}
