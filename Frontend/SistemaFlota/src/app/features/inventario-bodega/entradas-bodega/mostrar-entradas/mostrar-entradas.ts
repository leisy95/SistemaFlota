import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-mostrar-entradas',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule
  ],
  templateUrl: './mostrar-entradas.html',
  styleUrl: './mostrar-entradas.scss',
})
export class MostrarEntradas {
  rol = 'Administrador';

  modoAjuste = false;

  buscar = '';
  fechaFiltro = '';

  paginaActual = 1;
  pageSize = 5;

  seleccionadas: number[] = [];

  // Datos quemados
  entradas: any[] = [
    {
      id: 1,
      referencia: 'MAT-001',
      referenciaPeso: 'REF-P001',
      fechaRegistroItem: '2026-11-09T08:30:00',
      pesoActual: 250,
      estado: 'EN_ALMACEN'
    },
    {
      id: 2,
      referencia: 'MAT-002',
      referenciaPeso: 'REF-P002',
      fechaRegistroItem: '2026-11-08T10:15:00',
      pesoActual: 180,
      estado: 'EN_ALMACEN'
    },
    {
      id: 3,
      referencia: 'MAT-003',
      referenciaPeso: 'REF-P003',
      fechaRegistroItem: '2026-11-07T14:20:00',
      pesoActual: 320,
      estado: 'DESPACHADO'
    },
    {
      id: 4,
      referencia: 'MAT-004',
      referenciaPeso: 'REF-P004',
      fechaRegistroItem: '2026-11-06T09:00:00',
      pesoActual: 150,
      estado: 'EN_ALMACEN'
    },
    {
      id: 5,
      referencia: 'MAT-005',
      referenciaPeso: 'REF-P005',
      fechaRegistroItem: '2026-11-05T11:45:00',
      pesoActual: 400,
      estado: 'ANULADO'
    },
    {
      id: 6,
      referencia: 'MAT-006',
      referenciaPeso: 'REF-P006',
      fechaRegistroItem: '2026-11-04T16:10:00',
      pesoActual: 275,
      estado: 'EN_ALMACEN'
    },
    {
      id: 7,
      referencia: 'MAT-007',
      referenciaPeso: 'REF-P007',
      fechaRegistroItem: '2026-11-03T13:25:00',
      pesoActual: 190,
      estado: 'DESPACHADO'
    },
    {
      id: 8,
      referencia: 'MAT-008',
      referenciaPeso: 'REF-P008',
      fechaRegistroItem: '2026-11-02T08:50:00',
      pesoActual: 350,
      estado: 'EN_ALMACEN'
    },
    {
      id: 9,
      referencia: 'MAT-009',
      referenciaPeso: 'REF-P009',
      fechaRegistroItem: '2026-11-01T12:30:00',
      pesoActual: 210,
      estado: 'EN_ALMACEN'
    },
    {
      id: 10,
      referencia: 'MAT-010',
      referenciaPeso: 'REF-P010',
      fechaRegistroItem: '2026-10-30T15:40:00',
      pesoActual: 500,
      estado: 'ANULADO'
    }
  ];

  // FILTROS

  get entradasFiltradas(): any[] {
    return this.entradas.filter(item => {

      const texto = this.buscar.toLowerCase();

      const coincideTexto =
        item.referencia.toLowerCase().includes(texto) ||
        item.referenciaPeso.toLowerCase().includes(texto) ||
        item.estado.toLowerCase().includes(texto);

      const fechaItem = new Date(item.fechaRegistroItem)
        .toLocaleDateString('en-CA');

      const coincideFecha =
        !this.fechaFiltro || fechaItem === this.fechaFiltro;

      return coincideTexto && coincideFecha;
    });
  }

  // ESTADÍSTICAS

  get total(): number {
    return this.entradas.length;
  }

  get totalEnAlmacen(): number {
    return this.entradas.filter(
      item => item.estado === 'EN_ALMACEN'
    ).length;
  }

  get totalOtrosEstados(): number {
    return this.entradas.filter(
      item => item.estado !== 'EN_ALMACEN'
    ).length;
  }

  // PAGINACIÓN

  get totalPaginas(): number {
    return Math.max(
      1,
      Math.ceil(this.entradasFiltradas.length / this.pageSize)
    );
  }

  get registrosPaginados(): any[] {
    const inicio = (this.paginaActual - 1) * this.pageSize;

    return this.entradasFiltradas.slice(
      inicio,
      inicio + this.pageSize
    );
  }

  get registrosInicio(): number {
    return this.entradasFiltradas.length === 0
      ? 0
      : (this.paginaActual - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {
    return Math.min(
      this.paginaActual * this.pageSize,
      this.entradasFiltradas.length
    );
  }

  aplicarFiltro(event: Event): void {
    this.buscar = (event.target as HTMLInputElement).value;
    this.paginaActual = 1;
  }

  filtrarPorFecha(event: Event): void {
    this.fechaFiltro = (event.target as HTMLInputElement).value;
    this.paginaActual = 1;
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.fechaFiltro = '';
    this.paginaActual = 1;
  }

  cambiarPageSize(event: Event): void {
    this.pageSize = Number(
      (event.target as HTMLSelectElement).value
    );

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

  // AJUSTE GENERAL

  activarModoAjuste(): void {
    this.modoAjuste = true;
    this.seleccionadas = [];
  }

  cancelarAjuste(): void {
    this.modoAjuste = false;
    this.seleccionadas = [];
  }

  toggleSeleccion(item: any): void {
    if (this.estaSeleccionada(item)) {
      this.seleccionadas = this.seleccionadas.filter(
        id => id !== item.id
      );
    } else {
      this.seleccionadas.push(item.id);
    }
  }

  estaSeleccionada(item: any): boolean {
    return this.seleccionadas.includes(item.id);
  }

  confirmarAjusteGeneral(): void {
    if (this.rol !== 'Administrador' || !this.seleccionadas.length) {
      return;
    }

    console.log('Entradas seleccionadas:', this.seleccionadas);

    alert(
      `Ajuste general realizado para ${this.seleccionadas.length} entradas (simulación).`
    );

    this.cancelarAjuste();
  }

  // ACCIONES SIMULADAS

  eliminarItem(id: number): void {
    const confirmar = confirm(
      '¿Estás seguro de eliminar esta entrada?'
    );

    if (!confirmar) return;

    this.entradas = this.entradas.filter(
      item => item.id !== id
    );

    this.seleccionadas = this.seleccionadas.filter(
      itemId => itemId !== id
    );

    if (this.paginaActual > this.totalPaginas) {
      this.paginaActual = this.totalPaginas;
    }
  }

  reimprimirEtiqueta(item: any): void {
    alert(
      `Simulación de impresión de etiqueta: ${item.referencia}`
    );

    console.log('Etiqueta:', item);
  }

  // NUEVA ORDEN / DEVOLUCIONES NO APLICAN EN ESTE MÓDULO
}