import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CrearOrdenCompra } from '../crear-orden-compra/crear-orden-compra';
import { OrdenCompra } from '../../../../core/models/costos/ordenCompra/ordencompra.model';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';
import { AccionesOrdenCompra } from '../acciones-orden-compra/acciones-orden-compra';
import { Proveedor } from '../../../../core/models/costos/proveedores/proveedores.model';
import { ProveedorService } from '../../../../core/services/costos/proveedores/proveedor.service';
import { FiltrosOrdenCompra } from '../../../../core/models/costos/ordenCompra/filtrosordencompra.models';

@Component({
  selector: 'app-listar-orden-compra',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './listar-orden-compra.html',
  styleUrl: './listar-orden-compra.scss',
})
export class ListarOrdenCompra {
  buscar = '';
  proveedorFiltro?: number;
  estadoFiltro = '';
  ordenes: OrdenCompra[] = [];
  estados: string[] = [];
  proveedores: FiltrosOrdenCompra['proveedores'] = [];

  total = 0;
  pagina = 1;
  pageSize = 10;

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraService,
  ) { }

  ngOnInit(): void {
    this.obtenerFiltros();
    this.cargar();
  }

  obtenerFiltros(): void {
    this.ordenCompraService.obtenerFiltros().subscribe({
      next: (respuesta: FiltrosOrdenCompra) => {
        this.estados = respuesta.estados ?? [];
        this.proveedores = respuesta.proveedores ?? [];
      },
      error: err => {
        console.error('ERROR FILTROS:', err);
        this.toastr.error(
          'No fue posible cargar los filtros.',
          'Error'
        );
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
      panelClass: 'orden-compra-dialog'
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
        this.ordenes = resp.items.map(item => ({ ...item }));
        this.total = resp.total;
      },
      error: err => {
        console.error('ERROR:', err);
        this.toastr.error('No fue posible cargar las órdenes.');
      }
    });
  }

  verOrden(item: OrdenCompra): void {
    this.toastr.info(`Consultando ${item.numero}`, 'Orden');
  }

  abrirAcciones(item: OrdenCompra): void {
    const dialog = this.dialog.open(AccionesOrdenCompra, {
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
            panelClass: 'orden-compra-dialog',
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
          break;

        case 'recibir':
          break;

        case 'anular':
          break;
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