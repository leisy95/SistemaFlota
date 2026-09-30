import { CommonModule, DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { OrdenCompraNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-compras/OrdenCompraNoFormalizadaService';
import { ProveedorNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/proveedores/proveedor-no-formalizado.service';
import { OrdenCompraNoFormalizadaResponse } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-response.model';
import { DetalleRepmercancia } from '../detalle-repmercancia/detalle-repmercancia';

@Component({
  selector: 'app-listar-repmercancia',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DecimalPipe
  ],
  templateUrl: './listar-repmercancia.html',
  styleUrl: './listar-repmercancia.scss',
})
export class ListarRepmercancia {
  pagina = 1;
  pageSize = 10;
  total = 0;

  buscar = '';
  estado = '';
  proveedorId?: number;

  proveedores: any[] = [];
  estados: string[] = [];

  ordenSeleccionada: OrdenCompraNoFormalizadaResponse | null = null;
  ordenes: OrdenCompraNoFormalizadaResponse[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraNoFormalizadaService,
    private proveedorService: ProveedorNoFormalizadoService
  ) { }

  ngOnInit(): void {
    this.estados = [
      'Pendiente',
      'Parcial',
      'Recepcionada',
      'Confirmada'
    ];

    this.cargarProveedores();
    this.cargarOrdenes();
  }

  cargarProveedores(): void {
    this.proveedorService.obtenerParaRecepcion(
      '',
      'Activo',
      '',
      1,
      1000
    ).subscribe({
      next: (respuesta) => {
        this.proveedores = respuesta.datos;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar los proveedores no formalizados.',
          'Recepción'
        );
      }
    });
  }

  cargarOrdenes(): void {
    this.ordenCompraService.obtenerParaRecepcion(
      this.buscar,
      this.estado,
      this.proveedorId,
      this.pagina,
      this.pageSize
    ).subscribe({
      next: (resp) => {
        this.ordenes = resp.items;
        this.total = resp.total;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar las órdenes no formalizadas.',
          'Recepción'
        );
      }
    });
  }

  aplicarFiltros(): void {
    this.pagina = 1;
    this.ordenSeleccionada = null;
    this.cargarOrdenes();
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.estado = '';
    this.proveedorId = undefined;
    this.pagina = 1;
    this.ordenSeleccionada = null;
    this.cargarOrdenes();
  }

  buscarOrden(): void {
    this.ordenSeleccionada = null;
    this.cargarOrdenes();
  }

  get totalPaginas(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.pagina = pagina;
    this.cargarOrdenes();
  }

  abrirOrden(orden: OrdenCompraNoFormalizadaResponse): void {
    this.ordenSeleccionada = orden;

    this.toastr.success(
      `Orden ${orden.numero} seleccionada`,
      'Recepción no formalizada'
    );
  }

  verRecepcion(orden: OrdenCompraNoFormalizadaResponse): void {
    this.dialog.open(DetalleRepmercancia, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      data: {
        id: orden.recepcionId
      }
    }).afterClosed().subscribe(resultado => {
      if (resultado) {
        this.cargarOrdenes();
      }
    });
  }

  getClaseEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'pendiente':
        return 'estado-pendiente';
      case 'parcial':
        return 'estado-parcial';
      case 'recepcionada':
        return 'estado-recepcionada';
      case 'confirmada':
        return 'estado-confirmada';
      default:
        return 'estado-default';
    }
  }

  getIconoEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'pendiente':
        return 'fa-clock';
      case 'parcial':
        return 'fa-truck-ramp-box';
      case 'recepcionada':
        return 'fa-circle-check';
      case 'confirmada':
        return 'fa-circle-check';
      default:
        return 'fa-circle-info';
    }
  }

  getTextoBoton(
    orden: OrdenCompraNoFormalizadaResponse
  ): string {
    switch (orden.estado?.toLowerCase()) {
      case 'pendiente':
        return 'Iniciar Recepción';
      case 'parcial':
        return 'Continuar Recepción';
      case 'recepcionada':
        return 'Revisar y Confirmar Recepción';
      case 'confirmada':
        return 'Recepción Confirmada';
      default:
        return 'Iniciar Recepción';
    }
  }

  getIconoBoton(
    orden: OrdenCompraNoFormalizadaResponse
  ): string {
    switch (orden.estado?.toLowerCase()) {
      case 'pendiente':
        return 'fa-cube';
      case 'parcial':
        return 'fa-truck-ramp-box';
      case 'recepcionada':
        return 'fa-clipboard-check';
      case 'confirmada':
        return 'fa-circle-check';
      default:
        return 'fa-cube';
    }
  }

  puedeAccionar(
    orden: OrdenCompraNoFormalizadaResponse
  ): boolean {
    return orden.estado?.toLowerCase() !== 'confirmada';
  }
}