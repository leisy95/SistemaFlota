
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SalidaBodegaService } from '../../../../core/services/inventario-bodega/salidas-bodega/salida-bodega.service';
import { SalidaBodegaActual } from '../../../../core/models/inventario-bodega/salidas-bodega/salida-bodega.model';
import { ConfirmarSalidaBodegaDto, RespuestaSalidaBodegaDto } from '../../../../core/models/inventario-bodega/salidas-bodega/salida-bodega.dto';

interface Cliente {
  id: number;
  nombre: string;
  documento: string;
}

interface ProductoSalida {
  id: number;
  referencia: string;
  referenciaPeso: string;
  pesoActual: number;
}

@Component({
  selector: 'app-registro-salida',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './registro-salida.html',
  styleUrl: './registro-salida.scss',
})
export class RegistroSalida implements OnInit {

  idSalida: number | null = null;
  estadoSalida = '';

  cargando = false;
  procesando = false;

  filtroReferencia = '';

  clienteBusqueda = '';
  clienteSeleccionado: Cliente | null = null;
  mostrarClientes = false;

  paginaActual = 1;
  pageSize = 5;

  productosSeleccionados: ProductoSalida[] = [];

  clientes: Cliente[] = [
    { id: 1, nombre: 'Plásticos Industriales S.A.S.', documento: '900123456' },
    { id: 2, nombre: 'Empaques del Valle S.A.S.', documento: '901234567' },
    { id: 3, nombre: 'Distribuciones Nacionales', documento: '800123456' },
    { id: 4, nombre: 'Comercializadora Andina', documento: '900654321' },
    { id: 5, nombre: 'Industrias del Pacífico', documento: '901567890' },
    { id: 6, nombre: 'Empaques Flexibles S.A.S.', documento: '901345678' },
    { id: 7, nombre: 'Industrias Plásticas de Colombia', documento: '800987654' },
    { id: 8, nombre: 'Comercializadora Industrial', documento: '900456789' }
  ];

  constructor(
    private readonly salidaService: SalidaBodegaService,
    private readonly toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.cargarSalidaActual();
  }

  // CONSULTAR SALIDA ACTUAL

  cargarSalidaActual(): void {

    this.cargando = true;

    this.salidaService.obtenerSalidaActual().subscribe({

      next: (respuesta: SalidaBodegaActual) => {

        this.cargando = false;

        if (!respuesta.existe) {

          this.idSalida = null;
          this.estadoSalida = '';
          this.productosSeleccionados = [];

          return;
        }

        this.idSalida = respuesta.id ?? null;
        this.estadoSalida = respuesta.estado ?? '';

        this.productosSeleccionados = (respuesta.detalles ?? []).map(item => ({
          id: item.idInventarioBodegaItem,
          referencia: item.referenciaPeso,
          referenciaPeso: item.referenciaPeso,
          pesoActual: item.pesoActual
        }));

        this.ajustarPaginacion();
      },

      error: () => {

        this.cargando = false;

        this.toastr.error(
          'No fue posible consultar la salida actual.',
          'Error'
        );
      }
    });
  }

  private crearSalida(callback: (id: number) => void): void {

    this.procesando = true;

    this.salidaService.crearSalida().subscribe({

      next: (respuesta: RespuestaSalidaBodegaDto) => {

        this.procesando = false;

        if (!respuesta.exito || !respuesta.id) {

          this.toastr.warning(
            respuesta.mensaje,
            'No fue posible crear la salida'
          );

          return;
        }

        this.idSalida = respuesta.id;
        this.estadoSalida = 'EN_PROCESO';

        callback(respuesta.id);
      },

      error: () => {

        this.procesando = false;

        this.toastr.error(
          'No fue posible crear la salida.',
          'Error'
        );
      }
    });
  }

  // ESCANEAR REFERENCIA QR

  onScanChange(valor: string): void {

    const referencia = valor.trim().toUpperCase();

    if (!referencia || this.procesando || this.cargando) {
      return;
    }

    if (this.idSalida) {

      if (this.estadoSalida !== 'EN_PROCESO') {

        this.toastr.warning(
          'Esta salida ya no está disponible.',
          'Salida finalizada'
        );

        this.filtroReferencia = '';

        return;
      }

      this.agregarReferencia(this.idSalida, referencia);

      return;
    }

    this.crearSalida((id) => {
      this.agregarReferencia(id, referencia);
    });
  }

  // AGREGAR PRODUCTO POR REFERENCIA
  private agregarReferencia(idSalida: number, referencia: string): void {
    this.procesando = true;

    this.salidaService.agregarPorReferencia(idSalida, referencia).subscribe({
      next: (respuesta: RespuestaSalidaBodegaDto) => {
        this.procesando = false;
        this.filtroReferencia = '';

        if (!respuesta.exito) {
          this.toastr.warning(respuesta.mensaje, 'No se pudo agregar');
          return;
        }

        this.toastr.success(respuesta.mensaje, 'Producto agregado');
        this.cargarSalidaActual();
      },
      error: (error) => {
        this.procesando = false;
        this.filtroReferencia = '';
        this.toastr.error(error.error?.mensaje || 'No fue posible agregar el producto.', 'Error');
      }
    });
  }

  get clientesFiltrados(): Cliente[] {

    const texto = this.clienteBusqueda.toLowerCase().trim();

    if (!texto) {
      return this.clientes;
    }

    return this.clientes.filter(cliente =>
      cliente.nombre.toLowerCase().includes(texto) ||
      cliente.documento.includes(texto)
    );
  }

  seleccionarCliente(cliente: Cliente): void {

    this.clienteSeleccionado = cliente;
    this.clienteBusqueda = cliente.nombre;
    this.mostrarClientes = false;
  }

  abrirClientes(): void {
    this.mostrarClientes = true;
  }

  limpiarCliente(): void {

    this.clienteSeleccionado = null;
    this.clienteBusqueda = '';
    this.mostrarClientes = false;
  }


  get productosPaginados(): ProductoSalida[] {

    const inicio = (this.paginaActual - 1) * this.pageSize;

    return this.productosSeleccionados.slice(
      inicio,
      inicio + this.pageSize
    );
  }

  get totalProductos(): number {
    return this.productosSeleccionados.length;
  }

  get totalPeso(): number {

    return this.productosSeleccionados.reduce(
      (total, producto) => total + producto.pesoActual,
      0
    );
  }

  get totalPaginas(): number {

    return Math.ceil(this.totalProductos / this.pageSize) || 1;
  }

  get registrosInicio(): number {

    return this.totalProductos === 0
      ? 0
      : (this.paginaActual - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {

    return Math.min(
      this.paginaActual * this.pageSize,
      this.totalProductos
    );
  }

  private ajustarPaginacion(): void {

    if (this.paginaActual > this.totalPaginas) {
      this.paginaActual = this.totalPaginas;
    }
  }

  quitarSeleccionado(producto: ProductoSalida): void {

    if (!this.idSalida || this.procesando) {
      return;
    }

    this.procesando = true;

    this.salidaService.quitarItem(
      this.idSalida,
      producto.id
    ).subscribe({

      next: (respuesta: RespuestaSalidaBodegaDto) => {

        this.procesando = false;

        if (!respuesta.exito) {

          this.toastr.warning(respuesta.mensaje);

          return;
        }

        this.toastr.info(
          respuesta.mensaje,
          'Producto eliminado'
        );

        this.cargarSalidaActual();
      },

      error: () => {

        this.procesando = false;

        this.toastr.error(
          'No fue posible retirar el producto.',
          'Error'
        );
      }
    });
  }

  cambiarPageSize(): void {

    this.paginaActual = 1;
  }

  paginaAnterior(): void {

    if (this.paginaActual > 1) {
      this.paginaActual--;
    }
  }

  paginaSiguiente(): void {

    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
    }
  }

  confirmarSalida(): void {

    if (!this.idSalida) {

      this.toastr.warning(
        'No existe una salida para confirmar.'
      );

      return;
    }

    if (!this.clienteSeleccionado) {

      this.toastr.warning(
        'Debes seleccionar un cliente.',
        'Cliente requerido'
      );

      return;
    }

    if (this.productosSeleccionados.length === 0) {

      this.toastr.warning(
        'Debes agregar al menos un producto.',
        'Sin productos'
      );

      return;
    }

    if (this.procesando) {
      return;
    }

    const dto: ConfirmarSalidaBodegaDto = {
      clienteId: this.clienteSeleccionado.id
    };

    this.procesando = true;

    this.salidaService.confirmarSalida(
      this.idSalida,
      dto
    ).subscribe({

      next: (respuesta: RespuestaSalidaBodegaDto) => {

        this.procesando = false;

        if (!respuesta.exito) {

          this.toastr.warning(
            respuesta.mensaje,
            'No se pudo confirmar'
          );

          return;
        }

        this.toastr.success(
          respuesta.mensaje,
          'Salida confirmada'
        );

        this.limpiarFormulario();
      },

      error: (error) => {

        this.procesando = false;

        this.toastr.error(
          error.error?.mensaje ||
          'No fue posible confirmar la salida.',
          'Error'
        );
      }
    });
  }

  cancelarSalida(): void {

    if (!this.idSalida) {

      this.toastr.info(
        'No existe una salida en proceso.',
        'Sin cambios'
      );

      return;
    }

    if (this.procesando) {
      return;
    }

    this.procesando = true;

    this.salidaService.cancelarSalida(
      this.idSalida
    ).subscribe({

      next: (respuesta: RespuestaSalidaBodegaDto) => {

        this.procesando = false;

        if (!respuesta.exito) {

          this.toastr.warning(respuesta.mensaje);

          return;
        }

        this.toastr.info(
          respuesta.mensaje,
          'Salida cancelada'
        );

        this.limpiarFormulario();
      },

      error: () => {

        this.procesando = false;

        this.toastr.error(
          'No fue posible cancelar la salida.',
          'Error'
        );
      }
    });
  }

  private limpiarFormulario(): void {

    this.idSalida = null;
    this.estadoSalida = '';

    this.productosSeleccionados = [];

    this.filtroReferencia = '';

    this.limpiarCliente();

    this.paginaActual = 1;
  }
}
