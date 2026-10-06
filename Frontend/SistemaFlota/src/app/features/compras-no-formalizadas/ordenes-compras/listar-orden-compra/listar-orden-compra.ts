import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { CrearOrdenCompra } from '../crear-orden-compra/crear-orden-compra';
import { AccionesOrdenCompraNoFormalizada } from '../acciones-orden-compra-no-formalizada/acciones-orden-compra-no-formalizada';
import { DialogConfirmacion } from '../../../../shared/dialog-confirmacion/dialog-confirmacion';

import { OrdenCompraNoFormalizadaResponse } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-response.model';
import { FiltrosOrdenCompraNoFormalizada } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/filtrosordencompra-no-formalizada.model';
import { OrdenCompraNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-compras/OrdenCompraNoFormalizadaService';

@Component({
  selector: 'app-listar-orden-compra',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule
  ],
  templateUrl: './listar-orden-compra.html',
  styleUrl: './listar-orden-compra.scss',
})
export class ListarOrdenCompra {
  buscar = '';
  proveedorFiltro?: number;
  estadoFiltro = '';
  ordenes: OrdenCompraNoFormalizadaResponse[] = [];
  estados: string[] = [];
  proveedores: FiltrosOrdenCompraNoFormalizada['proveedores'] = [];
  total = 0;
  pagina = 1;
  pageSize = 10;

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraNoFormalizadaService
  ) { }

  ngOnInit(): void {
    this.obtenerFiltros();
    this.cargar();
  }

  obtenerFiltros(): void {
    this.ordenCompraService.obtenerFiltros().subscribe({
      next: (respuesta: FiltrosOrdenCompraNoFormalizada) => {
        this.estados = respuesta.estados ?? [];
        this.proveedores = respuesta.proveedores ?? [];
      },
      error: err => {
        console.error('ERROR FILTROS:', err);
        this.toastr.error('No fue posible cargar los filtros.', 'Error');
      }
    });
  }

  get pendientes(): number {
    return this.ordenes.filter(x => x.estado === 'Pendiente').length;
  }

  get recepcionada(): number {
    return this.ordenes.filter(x => x.estado === 'Recepcionada').length;
  }

  get parcial(): number {
    return this.ordenes.filter(x => x.estado === 'Parcial').length;
  }

  get confirmada(): number {
    return this.ordenes.filter(x => x.estado === 'Confirmada').length;
  }

  get valorTotal(): number {
    return this.ordenes.reduce((total, item) => total + item.totalPagar, 0);
  }

  nuevaOrden(): void {
    const dialog = this.dialog.open(CrearOrdenCompra, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      autoFocus: false,
      restoreFocus: false,
      panelClass: 'orden-compra-no-formalizada-dialog'
    });

    dialog.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.pagina = 1;
        this.cargar();
      }
    });
  }

  cargar(): void {
    this.ordenCompraService.obtener(
      this.pagina,
      this.pageSize,
      this.buscar,
      this.estadoFiltro,
      this.proveedorFiltro ? Number(this.proveedorFiltro) : undefined
    ).subscribe({
      next: resp => {
        this.ordenes = resp.items?.map(item => ({ ...item })) ?? [];
        this.total = resp.total ?? 0;
      },
      error: err => {
        console.error('ERROR:', err);
        this.toastr.error('No fue posible cargar las órdenes.');
      }
    });
  }

  verOrden(item: OrdenCompraNoFormalizadaResponse): void {
    this.toastr.info(`Consultando ${item.numero}`, 'Orden');
  }

  abrirAcciones(item: OrdenCompraNoFormalizadaResponse): void {
    const dialog = this.dialog.open(AccionesOrdenCompraNoFormalizada, {
      width: '380px',
      autoFocus: false,
      restoreFocus: false,
      disableClose: true,
      data: item
    });

    dialog.afterClosed().subscribe(accion => {
      if (!accion) return;

      switch (accion) {
        case 'detalle':
          this.verOrden(item);
          break;

        case 'editar':
          const dialogEditar = this.dialog.open(CrearOrdenCompra, {
            width: '1200px',
            maxWidth: '95vw',
            maxHeight: '95vh',
            disableClose: true,
            autoFocus: false,
            restoreFocus: false,
            panelClass: 'orden-compra-no-formalizada-dialog',
            data: {
              modo: 'editar',
              id: item.id
            }
          });

          dialogEditar.afterClosed().subscribe(resultado => {
            if (!resultado?.actualizado) return;
            this.cargar();
          });
          break;

        case 'imprimir':
          this.generarPdf(item);
          break;

        case 'recibir':
          break;

        case 'anular':
          this.confirmarAnulacion(item);
          break;
      }
    });
  }

  generarPdf(item: OrdenCompraNoFormalizadaResponse): void {
    this.ordenCompraService.generarPdf(item.id).subscribe({
      next: blob => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: () => {
        this.toastr.error('No fue posible generar el PDF.', 'Error');
      }
    });
  }

  confirmarAnulacion(item: OrdenCompraNoFormalizadaResponse): void {
    const dialog = this.dialog.open(DialogConfirmacion, {
      width: '450px',
      disableClose: true,
      data: {
        titulo: 'Anular orden de compra no formalizada',
        mensaje: `¿Está seguro de anular la orden ${item.numero}?`,
        textoConfirmar: 'Sí, anular',
        textoCancelar: 'Cancelar',
        tipo: 'warning'
      }
    });

    dialog.afterClosed().subscribe(confirmado => {
      if (!confirmado) return;
      this.anularOrden(item);
    });
  }

  anularOrden(item: OrdenCompraNoFormalizadaResponse): void {
    this.ordenCompraService.anular(item.id).subscribe({
      next: resp => {
        this.toastr.success(
          resp.mensaje,
          'Orden anulada'
        );
        this.cargar();
      },
      error: err => {
        const mensaje =
          err?.error?.mensaje ||
          'No fue posible anular la orden de compra no formalizada.';

        this.toastr.error(
          mensaje,
          'No se pudo anular'
        );
      }
    });
  }

  paginaAnterior(): void {
    if (this.pagina > 1) {
      this.pagina--;
      this.cargar();
    }
  }

  paginaSiguiente(): void {
    if (this.pagina < this.totalPaginas) {
      this.pagina++;
      this.cargar();
    }
  }

  get totalPaginas(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  get registrosInicio(): number {
    return this.total === 0 ? 0 : (this.pagina - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {
    return Math.min(this.pagina * this.pageSize, this.total);
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.proveedorFiltro = undefined;
    this.estadoFiltro = '';
    this.pagina = 1;
    this.cargar();
    this.toastr.success('Filtros limpiados', 'OK');
  }
}