
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Movimiento {
  id: number;
  fecha: Date;
  tipo: string;
  descripcion: string;
  usuario: string;
  referencia: string;
  detalles: string;
}

@Component({
  selector: 'app-historial-movimientos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './historial-movimientos.html',
  styleUrl: './historial-movimientos.scss',
})
export class HistorialMovimientos {

  buscar = '';
  tipoSeleccionado = '';
  usuarioSeleccionado = '';

  movimientos: Movimiento[] = [
    {
      id: 1,
      fecha: new Date(2026, 9, 28, 8, 15),
      tipo: 'Orden de compra',
      descripcion: 'Creación de orden de compra',
      usuario: 'admin',
      referencia: 'OC-2026-001',
      detalles: 'Se creó la orden de compra para el proveedor principal.'
    },
    {
      id: 2,
      fecha: new Date(2026, 9, 28, 8, 42),
      tipo: 'Recepción',
      descripcion: 'Recepción de materia prima',
      usuario: 'bodega',
      referencia: 'REC-2026-015',
      detalles: 'Se registró la entrada de material al inventario.'
    },
    {
      id: 3,
      fecha: new Date(2026, 9, 28, 9, 10),
      tipo: 'Costeo',
      descripcion: 'Actualización de costo promedio',
      usuario: 'contabilidad',
      referencia: 'CST-2026-008',
      detalles: 'Se actualizó el costo promedio del material recibido.'
    },
    {
      id: 4,
      fecha: new Date(2026, 9, 27, 14, 20),
      tipo: 'Orden de compra',
      descripcion: 'Actualización de orden de compra',
      usuario: 'admin',
      referencia: 'OC-2026-002',
      detalles: 'Se modificaron las cantidades de la orden de compra.'
    },
    {
      id: 5,
      fecha: new Date(2026, 9, 27, 15, 5),
      tipo: 'Recepción',
      descripcion: 'Registro de recepción de materiales',
      usuario: 'bodega',
      referencia: 'REC-2026-014',
      detalles: 'Se confirmó la recepción de los materiales solicitados.'
    },
    {
      id: 6,
      fecha: new Date(2026, 9, 26, 10, 30),
      tipo: 'Costeo',
      descripcion: 'Registro de costeo de inventario',
      usuario: 'contabilidad',
      referencia: 'CST-2026-007',
      detalles: 'Se registró el costeo correspondiente al inventario.'
    },
    {
      id: 7,
      fecha: new Date(2026, 9, 25, 11, 45),
      tipo: 'Orden de compra',
      descripcion: 'Creación de orden de compra',
      usuario: 'compras',
      referencia: 'OC-2026-003',
      detalles: 'Se generó una nueva orden de compra.'
    },
    {
      id: 8,
      fecha: new Date(2026, 9, 24, 16, 0),
      tipo: 'Recepción',
      descripcion: 'Entrada de materiales al almacén',
      usuario: 'bodega',
      referencia: 'REC-2026-013',
      detalles: 'Se registró el ingreso de materiales al almacén.'
    }
  ];

  get tipos(): string[] {
    return [...new Set(this.movimientos.map(m => m.tipo))];
  }

  get usuarios(): string[] {
    return [...new Set(this.movimientos.map(m => m.usuario))].sort();
  }

  get totalMovimientos(): number {
    return this.movimientos.length;
  }

  get totalOrdenesCompra(): number {
    return this.movimientos.filter(
      m => m.tipo === 'Orden de compra'
    ).length;
  }

  get totalRecepciones(): number {
    return this.movimientos.filter(
      m => m.tipo === 'Recepción'
    ).length;
  }

  get totalCosteos(): number {
    return this.movimientos.filter(
      m => m.tipo === 'Costeo'
    ).length;
  }

  get movimientosFiltrados(): Movimiento[] {
    const texto = this.buscar.trim().toLowerCase();

    return this.movimientos
      .filter(m => {
        const coincideTexto =
          !texto ||
          m.descripcion.toLowerCase().includes(texto) ||
          m.tipo.toLowerCase().includes(texto) ||
          m.usuario.toLowerCase().includes(texto) ||
          m.referencia.toLowerCase().includes(texto) ||
          m.detalles.toLowerCase().includes(texto);

        const coincideTipo =
          !this.tipoSeleccionado ||
          m.tipo === this.tipoSeleccionado;

        const coincideUsuario =
          !this.usuarioSeleccionado ||
          m.usuario === this.usuarioSeleccionado;

        return coincideTexto && coincideTipo && coincideUsuario;
      })
      .sort((a, b) => b.fecha.getTime() - a.fecha.getTime());
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.tipoSeleccionado = '';
    this.usuarioSeleccionado = '';
  }

  verDetalles(movimiento: Movimiento): void {
    window.alert(
      `Movimiento: ${movimiento.referencia}\n\n` +
      `Tipo: ${movimiento.tipo}\n` +
      `Usuario: ${movimiento.usuario}\n\n` +
      movimiento.detalles
    );
  }
}
