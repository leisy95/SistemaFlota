import { CommonModule, DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { IniciarRepmercancia } from '../iniciar-repmercancia/iniciar-repmercancia';
import { OrdenCompra } from '../../../../core/models/costos/ordenCompra/ordencompra.model';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';
import { DetalleRepmercancia } from '../detalle-repmercancia/detalle-repmercancia';
import { ProveedorService } from '../../../../core/services/costos/proveedores/proveedor.service';

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
  formaPago = '';
  fechaInicio?: string;
  fechaFin?: string;
  proveedores: any[] = [];
  estados: string[] = [];
  formasPago: string[] = [];
  ordenSeleccionada: any = null;
  ordenes: OrdenCompra[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraService,
    private proveedorService: ProveedorService
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
    this.proveedorService.obtener(
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
          'No fue posible cargar los proveedores.',
          'Recepción'
        );
      }
    });
  }

  cargarOrdenes(): void {
    this.ordenCompraService.obtener(
      this.pagina,
      this.pageSize,
      this.buscar,
      this.estado,
      this.proveedorId,
      this.formaPago,
      this.fechaInicio,
      this.fechaFin
    ).subscribe({
      next: (resp) => {
        this.ordenes = resp.items;
        this.total = resp.total;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar las órdenes.',
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


  buscarOrden() {
    this.ordenSeleccionada = null;
    this.cargarOrdenes();
  }

  get totalPaginas(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  cambiarPagina(pagina: number) {

    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.pagina = pagina;
    this.cargarOrdenes();
  }

  abrirOrden(orden: any) {

    this.ordenSeleccionada = orden;

    this.toastr.success(
      `Orden ${orden.numero} seleccionada`,
      'Recepción'
    );

  }

  verRecepcion(orden: any): void {

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

  iniciarRecepcion(): void {

    if (!this.ordenSeleccionada) {
      this.toastr.warning(
        'Seleccione una orden.',
        'Recepción'
      );
      return;
    }

    if (this.ordenSeleccionada.estado === 'Confirmada') {
      this.toastr.info(
        'Esta recepción ya fue confirmada.',
        'Recepción'
      );
      return;
    }

    const dialogRef = this.dialog.open(IniciarRepmercancia, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      autoFocus: false,
      data: this.ordenSeleccionada
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (!resultado) return;

      this.cargarOrdenes();

      this.toastr.success(
        'Recepción registrada correctamente.',
        'Recepción'
      );
    });
  }

  getClaseEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'pendiente': return 'estado-pendiente';
      case 'parcial': return 'estado-parcial';
      case 'recepcionada': return 'estado-recepcionada';
      case 'confirmada': return 'estado-confirmada';
      default: return 'estado-default';
    }
  }

  getIconoEstado(estado: string): string {
    switch (estado?.toLowerCase()) {
      case 'pendiente': return 'fa-clock';
      case 'parcial': return 'fa-truck-ramp-box';
      case 'recepcionada': return 'fa-circle-check';
      case 'confirmada': return 'fa-circle-check';
      default: return 'fa-circle-info';
    }
  }

  getTextoBoton(orden: OrdenCompra): string {

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

  getIconoBoton(orden: OrdenCompra): string {

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

  puedeAccionar(orden: OrdenCompra): boolean {
    return orden.estado?.toLowerCase() !== 'confirmada';
  }
}
