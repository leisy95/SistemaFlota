
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

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
    FormsModule
  ],
  templateUrl: './registro-salida.html',
  styleUrl: './registro-salida.scss',
})
export class RegistroSalida {

  // Búsqueda y escaneo
  filtroReferencia = '';

  // Cliente
  clienteBusqueda = '';
  clienteSeleccionado: Cliente | null = null;
  mostrarClientes = false;

  // Paginación
  paginaActual = 1;
  pageSize = 5;

  // Productos seleccionados
  productosSeleccionados: ProductoSalida[] = [];

  // Clientes quemados
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

  // Inventario de prueba
  inventario: ProductoSalida[] = [
    { id: 1, referencia: 'MAT-001', referenciaPeso: 'REF-P001', pesoActual: 25 },
    { id: 2, referencia: 'MAT-002', referenciaPeso: 'REF-P002', pesoActual: 30 },
    { id: 3, referencia: 'MAT-003', referenciaPeso: 'REF-P003', pesoActual: 45 },
    { id: 4, referencia: 'MAT-004', referenciaPeso: 'REF-P004', pesoActual: 20 },
    { id: 5, referencia: 'MAT-005', referenciaPeso: 'REF-P005', pesoActual: 50 },
    { id: 6, referencia: 'MAT-006', referenciaPeso: 'REF-P006', pesoActual: 35 },
    { id: 7, referencia: 'MAT-007', referenciaPeso: 'REF-P007', pesoActual: 40 },
    { id: 8, referencia: 'MAT-008', referenciaPeso: 'REF-P008', pesoActual: 15 }
  ];

  constructor(private toastr: ToastrService) { }

  // Clientes filtrados
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

  // Productos paginados
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

  // Escanear referencia
  onScanChange(valor: string): void {
    const referencia = valor.trim().toUpperCase();

    if (!referencia) return;

    const producto = this.inventario.find(item =>
      item.referenciaPeso.toUpperCase() === referencia ||
      item.referencia.toUpperCase() === referencia
    );

    if (!producto) return;

    const existe = this.productosSeleccionados.some(
      item => item.id === producto.id
    );

    if (existe) {
      this.toastr.warning(
        'Este producto ya fue agregado a la salida.',
        'Producto duplicado'
      );

      this.filtroReferencia = '';
      return;
    }

    this.productosSeleccionados.push({ ...producto });
    this.paginaActual = this.totalPaginas;

    this.toastr.success(
      `Referencia ${producto.referenciaPeso} agregada correctamente.`,
      'Producto agregado'
    );

    this.filtroReferencia = '';
  }

  // Seleccionar cliente
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

  // Eliminar producto
  quitarSeleccionado(producto: ProductoSalida): void {
    this.productosSeleccionados =
      this.productosSeleccionados.filter(
        item => item.id !== producto.id
      );

    if (this.paginaActual > this.totalPaginas) {
      this.paginaActual = this.totalPaginas;
    }

    this.toastr.info(
      'El producto fue retirado de la salida.',
      'Producto eliminado'
    );
  }

  // Paginación
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

  // Confirmar salida
  confirmarSalida(): void {
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

    this.toastr.success(
      `Salida registrada correctamente para ${this.clienteSeleccionado.nombre}.`,
      'Salida confirmada'
    );

    this.limpiarFormulario();
  }

  // Cancelar salida
  cancelarSalida(): void {
    if (
      this.productosSeleccionados.length === 0 &&
      !this.clienteSeleccionado
    ) {
      this.toastr.info(
        'No hay información para cancelar.',
        'Sin cambios'
      );
      return;
    }

    this.limpiarFormulario();

    this.toastr.info(
      'La salida fue cancelada.',
      'Operación cancelada'
    );
  }

  // Limpiar formulario
  private limpiarFormulario(): void {
    this.productosSeleccionados = [];
    this.filtroReferencia = '';
    this.limpiarCliente();
    this.paginaActual = 1;
  }
}