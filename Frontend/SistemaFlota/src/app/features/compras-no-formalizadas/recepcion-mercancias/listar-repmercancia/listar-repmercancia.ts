import { CommonModule, DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { OrdenCompraNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-compras/OrdenCompraNoFormalizadaService';
import { ProveedorNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/proveedores/proveedor-no-formalizado.service';
import { RecepcionMercanciaNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/recepcion-mercancias/recepcion-mercancianoformalizada.service';

import { OrdenCompraNoFormalizadaResponse } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-response.model';

import { DetalleRepmercancia } from '../detalle-repmercancia/detalle-repmercancia';
import { IniciarRepmercancia } from '../iniciar-repmercancia/iniciar-repmercancia';
import { AccionesRecepcionDialogNoFormalizada } from '../acciones-recepcion-dialog-no-formalizada/acciones-recepcion-dialog-no-formalizada';

@Component({
  selector: 'app-listar-repmercancia',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DecimalPipe
  ],
  templateUrl: './listar-repmercancia.html',
  styleUrl: './listar-repmercancia.scss'
})
export class ListarRepmercancia {
  pagina = 1;
  pageSize = 10;
  total = 0;

  buscar = '';
  estado = '';
  proveedorNoFormalizadoId: number | undefined = undefined;

  proveedores: any[] = [];
  estados: string[] = [
    'Pendiente',
    'Parcial',
    'Recepcionada',
    'Confirmada',
    'Anulada'
  ];

  ordenes: OrdenCompraNoFormalizadaResponse[] = [];
  ordenSeleccionada: OrdenCompraNoFormalizadaResponse | null = null;
  cargando = false;

  constructor(
    private ordenCompraService: OrdenCompraNoFormalizadaService,
    private proveedorService: ProveedorNoFormalizadoService,
    private recepcionService: RecepcionMercanciaNoFormalizadaService,
    private dialog: MatDialog,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.cargarProveedores();
    this.cargarOrdenes();
  }

  cargarProveedores(): void {
    this.proveedorService.obtenerParaRecepcion('', 'Activo', '', 1, 1000).subscribe({
      next: (respuesta: any) => {
        this.proveedores = respuesta?.datos ?? [];
      },
      error: () => {
        this.proveedores = [];
        this.toastr.error('No fue posible cargar los proveedores no formalizados.');
      }
    });
  }

  cargarOrdenes(): void {
    this.cargando = true;

    this.ordenCompraService.obtenerParaRecepcion(
      this.buscar || '',
      this.estado || '',
      this.proveedorNoFormalizadoId,
      this.pagina,
      this.pageSize
    ).subscribe({
      next: (respuesta: any) => {
        this.ordenes = respuesta?.items ?? [];
        this.total = respuesta?.total ?? 0;
        this.cargando = false;
      },
      error: () => {
        this.ordenes = [];
        this.total = 0;
        this.cargando = false;
        this.toastr.error('No fue posible cargar las órdenes de compra no formalizadas.');
      }
    });
  }

  buscarOrden(): void {
    this.pagina = 1;
    this.cargarOrdenes();
  }

  aplicarFiltros(): void {
    this.pagina = 1;
    this.cargarOrdenes();
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.estado = '';
    this.proveedorNoFormalizadoId = undefined;
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

  get totalPaginas(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  abrirOrden(orden: OrdenCompraNoFormalizadaResponse): void {
    this.ordenSeleccionada = orden;
  }

  verRecepcion(orden: OrdenCompraNoFormalizadaResponse): void {
    if (!orden.recepcionId) {
      this.toastr.warning(
        'No se encontró la recepción asociada.',
        'Recepción'
      );
      return;
    }

    this.dialog.open(DetalleRepmercancia, {
      width: '95%',
      maxWidth: '1200px',
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

  verPdfRecepcion(orden: OrdenCompraNoFormalizadaResponse): void {
    if (!orden.recepcionId) {
      this.toastr.warning(
        'No se encontró la recepción asociada.',
        'Recepción'
      );
      return;
    }

    this.recepcionService.obtenerPdf(orden.recepcionId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.toastr.error(
          'No fue posible generar el PDF de la recepción.'
        );
      }
    });
  }

  generarEtiquetas(orden: OrdenCompraNoFormalizadaResponse): void {
    if (!orden.recepcionId) {
      this.toastr.warning(
        'No se encontró la recepción asociada.',
        'Recepción'
      );
      return;
    }

    this.recepcionService.obtenerEtiquetas(orden.recepcionId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        window.URL.revokeObjectURL(url);
      },
      error: () => {
        this.toastr.error(
          'No fue posible generar las etiquetas.'
        );
      }
    });
  }

  iniciarRecepcion(orden?: OrdenCompraNoFormalizadaResponse): void {
    if (orden) {
      this.ordenSeleccionada = orden;
    }

    if (!this.ordenSeleccionada) {
      this.toastr.warning('Seleccione una orden de compra.');
      return;
    }

    const dialogRef = this.dialog.open(IniciarRepmercancia, {
      width: '95%',
      maxWidth: '1200px',
      disableClose: true,
      data: {
        orden: this.ordenSeleccionada
      }
    });

    dialogRef.afterClosed().subscribe((resultado) => {
      if (resultado) {
        this.cargarOrdenes();
        this.ordenSeleccionada = null;
      }
    });
  }

  abrirAcciones(orden: OrdenCompraNoFormalizadaResponse): void {
    const dialogRef = this.dialog.open(AccionesRecepcionDialogNoFormalizada, {
      width: '500px',
      data: {
        orden
      }
    });

    dialogRef.afterClosed().subscribe((accion: string) => {
      if (!accion) {
        return;
      }

      switch (accion) {
        case 'ver':
          this.verRecepcion(orden);
          break;

        case 'pdf':
          this.verPdfRecepcion(orden);
          break;

        case 'etiquetas':
          this.generarEtiquetas(orden);
          break;

        case 'iniciar':
        case 'continuar':
          this.ordenSeleccionada = orden;
          this.iniciarRecepcion();
          break;
      }
    });
  }

  getClaseEstado(estado: string | null | undefined): string {
    switch (estado?.trim()?.toLowerCase()) {
      case 'pendiente':
        return 'estado-pendiente';

      case 'parcial':
        return 'estado-parcial';

      case 'recepcionada':
      case 'confirmada':
        return 'estado-confirmada';

      case 'anulada':
        return 'estado-anulada';

      default:
        return '';
    }
  }

  getIconoEstado(estado: string | null | undefined): string {
    switch (estado?.trim()?.toLowerCase()) {
      case 'pendiente':
        return 'fa-clock';

      case 'parcial':
        return 'fa-truck-ramp-box';

      case 'recepcionada':
        return 'fa-box-open';

      case 'confirmada':
        return 'fa-circle-check';

      case 'anulada':
        return 'fa-ban';

      default:
        return 'fa-circle-question';
    }
  }

  getTextoBoton(orden: OrdenCompraNoFormalizadaResponse): string {
    const estado = orden.estado?.trim()?.toLowerCase();

    switch (estado) {
      case 'pendiente':
        return 'Iniciar Recepción';

      case 'parcial':
        return 'Continuar Recepción';

      case 'recepcionada':
        return 'Ver Recepción';

      case 'confirmada':
        return 'Ver Recepción';

      case 'anulada':
        return 'Ver Orden';

      default:
        return 'Ver';
    }
  }

  getIconoBoton(orden: OrdenCompraNoFormalizadaResponse): string {
    const estado = orden.estado?.trim()?.toLowerCase();

    switch (estado) {
      case 'pendiente':
        return 'fa-play';

      case 'parcial':
        return 'fa-truck-ramp-box';

      case 'recepcionada':
      case 'confirmada':
      case 'anulada':
        return 'fa-eye';

      default:
        return 'fa-eye';
    }
  }

  puedeAccionar(orden: OrdenCompraNoFormalizadaResponse): boolean {
    const estado = orden.estado?.trim()?.toLowerCase();

    return estado !== 'confirmada' && estado !== 'anulada';
  }
}