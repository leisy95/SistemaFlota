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


@Component({
  selector: 'app-listar-orden-compra',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
  ],
  templateUrl: './listar-orden-compra.html',
  styleUrl: './listar-orden-compra.scss',
})
export class ListarOrdenCompra {

  buscar = '';
  proveedorFiltro?: number;
  estadoFiltro = '';
  ordenes: OrdenCompra[] = [];
  proveedores: Proveedor[] = [];

  total = 0;
  pagina = 1;
  pageSize = 10;

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraService,
    private proveedorService: ProveedorService
  ) { }

  ngOnInit(): void {
    this.obtenerProveedores();
    this.cargar();
  }

  get pendientes(): number {
    return this.ordenes.filter(x => x.estado === 'Pendiente').length;
  }

  get recibidas(): number {
    return this.ordenes.filter(x => x.estado === 'Recibida').length;
  }

  get valorTotal(): number {
    return this.ordenes.reduce(
      (total, item) => total + item.totalPagar,
      0
    );
  }

  obtenerProveedores(): void {

    this.proveedorService
      .obtener('', '', 'nombre', 1, 1000)
      .subscribe({

        next: (respuesta) => {
          this.proveedores = respuesta.datos;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar los proveedores.',
            'Error'
          );
        }
      });
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

    this.ordenCompraService
      .obtener(
        this.pagina,
        this.pageSize,
        this.buscar,
        this.estadoFiltro,
        this.proveedorFiltro ? Number(this.proveedorFiltro) : undefined
      )
      .subscribe({

        next: resp => {
          this.ordenes = resp.items;
          this.total = resp.total;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar las órdenes.'
          );

        }
      });

  }

  verOrden(item: OrdenCompra): void {
    console.log(item);

    this.toastr.info(
      `Consultando ${item.numero}`,
      'Orden'
    );
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
            if (resultado) {
              this.cargar();
            }
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
    return this.total === 0
      ? 0
      : (this.pagina - 1) * this.pageSize + 1;
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
    this.toastr.success(
      'Filtros limpiados',
      'OK'
    );
  }
}
