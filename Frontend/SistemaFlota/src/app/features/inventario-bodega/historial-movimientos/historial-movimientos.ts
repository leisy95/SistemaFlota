import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { HistorialInventarioBodegaService } from '../../../core/services/inventario-bodega/historial-movimientos/historial-inventario-bodega.service';
import { FiltroHistorialInventarioBodega, MovimientoInventarioBodega, MovimientoInventarioBodegaOpciones, MovimientoInventarioBodegaResumen } from '../../../core/models/inventario-bodega/historial-movimientos/historial-inventario-bodega.model';

@Component({
  selector: 'app-historial-movimientos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './historial-movimientos.html',
  styleUrl: './historial-movimientos.scss',
})
export class HistorialMovimientos implements OnInit {

  private historialService = inject(HistorialInventarioBodegaService);
  private toastr = inject(ToastrService);

  movimientos: MovimientoInventarioBodega[] = [];

  resumen: MovimientoInventarioBodegaResumen = {
    totalMovimientos: 0,
    totalCreaciones: 0,
    totalActualizaciones: 0,
    totalEliminaciones: 0,
    totalConfirmaciones: 0,
    totalCancelaciones: 0
  };

  opciones: MovimientoInventarioBodegaOpciones = {
    modulos: [],
    usuarios: [],
    acciones: []
  };

  pagina = 1;
  porPagina = 10;
  totalRegistros = 0;
  totalPaginas = 0;

  cargando = false;

  filtros = {
    desde: '',
    hasta: '',
    modulo: '',
    accion: '',
    usuario: '',
    buscar: ''
  };

  ngOnInit(): void {
    this.cargarOpciones();
    this.cargarHistorial();
  }

  cargarOpciones(): void {
    this.historialService.obtenerOpciones().subscribe({
      next: (resp) => {
        this.opciones = resp;
      },
      error: () => {
        this.toastr.error('No fue posible cargar las opciones del historial.');
      }
    });
  }

  cargarHistorial(): void {
    this.cargando = true;

    const filtros: FiltroHistorialInventarioBodega = {
      desde: this.filtros.desde || undefined,
      hasta: this.filtros.hasta || undefined,
      modulo: this.filtros.modulo || undefined,
      accion: this.filtros.accion || undefined,
      usuario: this.filtros.usuario || undefined,
      buscar: this.filtros.buscar.trim() || undefined,
      pagina: this.pagina,
      porPagina: this.porPagina
    };

    this.historialService.obtener(filtros).subscribe({
      next: (resp) => {
        this.movimientos = resp.movimientos;
        this.totalRegistros = resp.totalRegistros;
        this.totalPaginas = resp.totalPaginas;
        this.cargando = false;
      },
      error: () => {
        this.cargando = false;
        this.toastr.error('No fue posible cargar el historial de movimientos.');
      }
    });

    this.cargarResumen();
  }

  cargarResumen(): void {
    const filtros = {
      desde: this.filtros.desde || undefined,
      hasta: this.filtros.hasta || undefined,
      modulo: this.filtros.modulo || undefined,
      accion: this.filtros.accion || undefined,
      usuario: this.filtros.usuario || undefined,
      buscar: this.filtros.buscar.trim() || undefined
    };

    this.historialService.obtenerResumen(filtros).subscribe({
      next: (resp) => {
        this.resumen = resp;
      },
      error: () => {
        this.toastr.error('No fue posible cargar el resumen del historial.');
      }
    });
  }

  aplicarFiltros(): void {
    this.pagina = 1;
    this.cargarHistorial();
  }

  limpiarFiltros(): void {
    this.filtros = {
      desde: '',
      hasta: '',
      modulo: '',
      accion: '',
      usuario: '',
      buscar: ''
    };

    this.pagina = 1;
    this.cargarHistorial();
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.pagina = pagina;
    this.cargarHistorial();
  }

  cambiarPorPagina(): void {
    this.pagina = 1;
    this.cargarHistorial();
  }

  obtenerClaseAccion(accion: string): string {
    switch (accion.toLowerCase()) {
      case 'crear':
      case 'confirmar':
        return 'estado-exito';

      case 'actualizar':
      case 'editar':
      case 'agregaritem':
        return 'estado-info';

      case 'eliminar':
      case 'cancelar':
      case 'quitaritem':
        return 'estado-error';

      case 'devolucion':
      case 'ajustegeneral':
      case 'recalcular':
        return 'estado-warning';

      default:
        return 'estado-default';
    }
  }

  formatearFecha(fecha: string): string {
    return new Date(fecha).toLocaleString('es-CO', {
      dateStyle: 'short',
      timeStyle: 'short'
    });
  }

  get hayMovimientos(): boolean {
    return this.movimientos.length > 0;
  }
}