import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { FormsModule } from '@angular/forms';

import { CrearTraslado } from '../crear-traslado/crear-traslado';
import { VerTraslado } from '../ver-traslado/ver-traslado';

import { OrdenTrasladoService } from '../../../../core/services/costos/ordenestraslado/ordentraslado.service';
import {
  OrdenTraslado,
  OrdenTrasladoPaginado
} from '../../../../core/models/costos/OrdenesTraslado/orden-traslado.model';
import { VerificarOrdenTraslado } from '../verificar-orden-traslado/verificar-orden-traslado';
import { DialogConfirmacion, DialogConfirmacionData } from '../../../../shared/dialog-confirmacion/dialog-confirmacion';

@Component({
  selector: 'app-listar-traslado',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './listar-traslado.html',
  styleUrl: './listar-traslado.scss',
})
export class ListarTraslado implements OnInit {

  ordenes: OrdenTraslado[] = [];

  search = '';
  estado = '';
  destino = '';
  fechaInicio = '';
  fechaFin = '';

  pagina = 1;
  tamanoPagina = 10;
  totalRegistros = 0;
  totalPaginas = 0;

  cargando = false;

  constructor(
    private dialog: MatDialog,
    private toastr: ToastrService,
    private ordenTrasladoService: OrdenTrasladoService
  ) { }

  ngOnInit(): void {
    this.cargarOrdenes();
  }

  cargarOrdenes(): void {
    this.cargando = true;

    this.ordenTrasladoService.obtenerTodos(
      this.search,
      this.estado,
      this.destino,
      this.fechaInicio,
      this.fechaFin,
      this.pagina,
      this.tamanoPagina
    ).subscribe({
      next: (respuesta: OrdenTrasladoPaginado) => {
        this.ordenes = respuesta.datos;
        this.totalRegistros = respuesta.totalRegistros;
        this.pagina = respuesta.pagina;
        this.tamanoPagina = respuesta.tamanoPagina;
        this.totalPaginas = respuesta.totalPaginas;
        this.cargando = false;
      },
      error: (error) => {
        this.cargando = false;

        this.toastr.error(
          error?.error?.mensaje || 'No fue posible cargar las órdenes.',
          'Error'
        );
      }
    });
  }

  buscar(): void {
    this.pagina = 1;
    this.cargarOrdenes();
  }

  limpiarFiltros(): void {
    this.search = '';
    this.estado = '';
    this.destino = '';
    this.fechaInicio = '';
    this.fechaFin = '';
    this.pagina = 1;

    this.cargarOrdenes();
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.pagina = pagina;
    this.cargarOrdenes();
  }

  cambiarTamanoPagina(): void {
    this.pagina = 1;
    this.cargarOrdenes();
  }

  get paginas(): number[] {
    const paginas: number[] = [];

    const inicio = Math.max(1, this.pagina - 2);
    const fin = Math.min(this.totalPaginas, this.pagina + 2);

    for (let i = inicio; i <= fin; i++) {
      paginas.push(i);
    }

    return paginas;
  }

  nuevaOrden(): void {
    const dialogRef = this.dialog.open(CrearTraslado, {
      width: '1200px',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '95vh',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.cargarOrdenes();

        this.toastr.success(
          `Orden ${resultado.numeroOrden} creada correctamente.`,
          'Orden de traslado'
        );
      }
    });
  }

  verOrden(orden: OrdenTraslado): void {
    const dialogRef = this.dialog.open(VerTraslado, {
      width: '1200px',
      maxWidth: '95vw',
      height: '90vh',
      maxHeight: '95vh',
      data: orden,
      disableClose: false,
      panelClass: 'dialog-ver-traslado'
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.cargarOrdenes();
      }
    });
  }

  obtenerClaseEstado(estado: string): string {
    switch (estado?.toLowerCase()) {

      case 'completado':
      case 'confirmado':
        return 'completada';

      case 'en proceso':
      case 'verificando':
        return 'proceso';

      case 'pendiente':
        return 'pendiente';

      case 'anulado':
        return 'anulada';

      default:
        return 'pendiente';
    }
  }

  iniciarVerificacion(orden: any) {
    const dialogRef = this.dialog.open(VerificarOrdenTraslado, {
      width: '1000px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      data: orden
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.cargarOrdenes();
      }
    });
  }

  confirmarOrden(orden: OrdenTraslado): void {
    if (orden.estado !== 'Verificando') {
      return;
    }

    const dialogRef = this.dialog.open(DialogConfirmacion, {
      width: '450px',
      maxWidth: '95vw',
      disableClose: true,
      data: {
        titulo: 'Confirmar orden de traslado',
        mensaje: `¿Está seguro de confirmar la orden ${orden.numeroOrden}?`,
        textoConfirmar: 'Sí, confirmar',
        textoCancelar: 'Cancelar',
        tipo: 'warning'
      } as DialogConfirmacionData
    });

    dialogRef.afterClosed().subscribe(confirmado => {
      if (!confirmado) {
        return;
      }

      this.ordenTrasladoService.confirmar(orden.id).subscribe({
        next: () => {
          this.toastr.success(
            `La orden ${orden.numeroOrden} fue confirmada correctamente.`,
            'Orden de traslado'
          );

          this.cargarOrdenes();
        },
        error: error => {
          this.toastr.error(
            error?.error?.mensaje ||
            'No fue posible confirmar la orden.',
            'Error'
          );
        }
      });
    });
  }
}