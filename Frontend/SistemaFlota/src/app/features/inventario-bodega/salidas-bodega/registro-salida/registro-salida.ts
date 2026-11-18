
import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { SalidaBodegaService } from '../../../../core/services/inventario-bodega/salidas-bodega/salida-bodega.service';
import { ClienteBodegaService } from '../../../../core/services/inventario-bodega/clientes/cliente-bodega.service';
import { SalidaBodegaActual } from '../../../../core/models/inventario-bodega/salidas-bodega/salida-bodega.model';
import { ConfirmarSalidaBodegaDto, RespuestaSalidaBodegaDto } from '../../../../core/models/inventario-bodega/salidas-bodega/salida-bodega.dto';
import { ClienteBodega } from '../../../../core/models/inventario-bodega/clientes/cliente-bodega.model';
import { BrowserMultiFormatReader, IScannerControls } from '@zxing/browser';

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
export class RegistroSalida implements OnInit, OnDestroy {

  @ViewChild('inputRef') inputRef!: ElementRef<HTMLInputElement>;

  // ESCÁNER CON CÁMARA
  @ViewChild('videoEscaner')
  videoEscaner!: ElementRef<HTMLVideoElement>;

  private codeReader = new BrowserMultiFormatReader();

  private scannerControls?: IScannerControls;

  mostrarEscaner = false;
  iniciandoCamara = false;

  referenciasEscaneadas: string[] = [];

  // Evitar lecturas repetidas consecutivas
  private ultimaReferenciaCamara = '';
  private ultimaLecturaCamara = 0;

  private scanTimeout: ReturnType<typeof setTimeout> | null = null;

  ultimoCodigoLeido = '';

  idSalida: number | null = null;
  estadoSalida = '';

  cargando = false;
  procesando = false;

  filtroReferencia = '';

  // CLIENTES
  clienteBusqueda = '';
  clienteSeleccionado: ClienteBodega | null = null;
  mostrarClientes = false;

  clientes: ClienteBodega[] = [];
  cargandoClientes = false;

  paginaActual = 1;
  pageSize = 5;

  productosSeleccionados: ProductoSalida[] = [];

  constructor(
    private readonly salidaService: SalidaBodegaService,
    private readonly clienteService: ClienteBodegaService,
    private readonly toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.cargarSalidaActual();
    this.cargarClientes();
  }

  // ABRIR ESCÁNER DE CÁMARA
  async abrirEscaner(): Promise<void> {
    if (this.mostrarEscaner) return;

    this.mostrarEscaner = true;
    this.iniciandoCamara = true;

    setTimeout(async () => {
      try {
        this.scannerControls = await this.codeReader.decodeFromVideoDevice(
          undefined,
          this.videoEscaner.nativeElement,
          (resultado) => {
            if (!resultado) return;
            const referencia = resultado.getText().trim().toUpperCase();
            this.registrarReferenciaEscaneada(referencia);
          }
        );
      } catch (error) {
        console.error('Error al iniciar la cámara:', error);
        this.toastr.error('No fue posible acceder a la cámara. Verifica los permisos del dispositivo.', 'Error');
        this.detenerEscaner();
      } finally {
        this.iniciandoCamara = false;
      }
    }, 300);
  }

  // REGISTRAR REFERENCIA LEÍDA POR LA CÁMARA
  private registrarReferenciaEscaneada(
    referencia: string
  ): void {

    if (!referencia) return;

    const ahora = Date.now();

    // Evitar lecturas consecutivas del mismo código
    if (
      referencia === this.ultimaReferenciaCamara &&
      ahora - this.ultimaLecturaCamara < 1200
    ) {
      return;
    }

    this.ultimaReferenciaCamara = referencia;
    this.ultimaLecturaCamara = ahora;

    // Evitar referencias duplicadas en la lista temporal
    if (this.referenciasEscaneadas.includes(referencia)) {
      return;
    }

    // Evitar referencias que ya están en la salida
    const yaExiste = this.productosSeleccionados.some(
      producto => producto.referenciaPeso === referencia
    );

    if (yaExiste) {

      this.toastr.warning(
        `La referencia ${referencia} ya está agregada a la salida.`,
        'Producto duplicado'
      );

      return;
    }

    this.referenciasEscaneadas = [
      ...this.referenciasEscaneadas,
      referencia
    ];
  }


  // DETENER CÁMARA
  detenerEscaner(): void {

    this.scannerControls?.stop();

    this.scannerControls = undefined;

    this.mostrarEscaner = false;
    this.iniciandoCamara = false;

    this.ultimaReferenciaCamara = '';
    this.ultimaLecturaCamara = 0;
  }


  // ELIMINAR REFERENCIA DE LA LISTA TEMPORAL
  eliminarReferenciaEscaneada(referencia: string): void {

    this.referenciasEscaneadas =
      this.referenciasEscaneadas.filter(
        item => item !== referencia
      );
  }


  // LIMPIAR REFERENCIAS ESCANEADAS
  limpiarReferenciasEscaneadas(): void {

    this.referenciasEscaneadas = [];

  }

  // CERRAR RECURSOS AL SALIR DEL COMPONENTE
  ngOnDestroy(): void {

    this.detenerEscaner();

    if (this.scanTimeout) {
      clearTimeout(this.scanTimeout);
    }
  }

  get totalReferenciasEscaneadas(): number {
    return this.referenciasEscaneadas.length;
  }

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

  cargarClientes(buscar: string = ''): void {

    this.cargandoClientes = true;

    this.clienteService.listarClientesParaSalidas({
      buscar: buscar.trim(),
      nombre: '',
      telefono: '',
      email: '',
      activo: true,
      pagina: 1,
      pageSize: 100
    }).subscribe({

      next: (respuesta) => {

        this.clientes = respuesta.items ?? [];
        this.cargandoClientes = false;

      },

      error: () => {

        this.clientes = [];
        this.cargandoClientes = false;

        this.toastr.error(
          'No fue posible cargar los clientes.',
          'Error'
        );
      }
    });
  }

  get clientesFiltrados(): ClienteBodega[] {
    return this.clientes;
  }

  abrirClientes(): void {

    this.mostrarClientes = true;

    if (this.clientes.length === 0 && !this.cargandoClientes) {
      this.cargarClientes(this.clienteBusqueda);
    }
  }

  buscarCliente(valor: string): void {

    this.clienteBusqueda = valor;
    this.clienteSeleccionado = null;
    this.mostrarClientes = true;

    this.cargarClientes(valor);
  }

  seleccionarCliente(cliente: ClienteBodega): void {

    this.clienteSeleccionado = cliente;
    this.clienteBusqueda = cliente.nombre;
    this.mostrarClientes = false;
  }

  limpiarCliente(): void {

    this.clienteSeleccionado = null;
    this.clienteBusqueda = '';
    this.mostrarClientes = false;

    this.cargarClientes();
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

  private resetInput(): void {
    this.filtroReferencia = '';

    setTimeout(() => {
      this.inputRef?.nativeElement.focus();
    }, 0);
  }

  // ESCANEAR REFERENCIA QR

  onScanChange(valor: string): void {

    if (this.scanTimeout) {
      clearTimeout(this.scanTimeout);
    }

    if (!valor) return;

    // Esperar a que el lector termine de escribir la referencia
    this.scanTimeout = setTimeout(() => {

      const referencia = valor.trim().toUpperCase();

      if (!referencia || referencia === this.ultimoCodigoLeido) {
        return;
      }

      if (this.procesando || this.cargando) {
        return;
      }

      this.ultimoCodigoLeido = referencia;

      if (this.idSalida) {

        if (this.estadoSalida !== 'EN_PROCESO') {

          this.toastr.warning(
            'Esta salida ya no está disponible.',
            'Salida finalizada'
          );

          this.resetInput();
          return;
        }

        this.agregarReferencia(this.idSalida, referencia);

        return;
      }

      this.crearSalida((id) => {
        this.agregarReferencia(id, referencia);
      });

      // Permitir escanear nuevamente la misma referencia
      setTimeout(() => {
        this.ultimoCodigoLeido = '';
      }, 1000);

    }, 300);
  }

  // AGREGAR PRODUCTO POR REFERENCIA

  private agregarReferencia(idSalida: number, referencia: string): void {

    this.procesando = true;

    this.salidaService.agregarPorReferencia(idSalida, referencia).subscribe({

      next: (respuesta: RespuestaSalidaBodegaDto) => {

        this.procesando = false;
        this.filtroReferencia = '';

        if (!respuesta.exito) {

          this.toastr.warning(
            respuesta.mensaje,
            'No se pudo agregar'
          );

          return;
        }

        this.toastr.success(
          respuesta.mensaje,
          'Producto agregado'
        );

        this.cargarSalidaActual();
      },

      error: (error) => {

        this.procesando = false;
        this.resetInput();

        this.toastr.error(
          error.error?.mensaje ||
          'No fue posible agregar el producto.',
          'Error'
        );
      }
    });
  }

  // PRODUCTOS PAGINADOS

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

  // CONFIRMAR SALIDA

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
