
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MovimientoInventarioAuditoria,
  MovimientoInventarioAuditoriaCompleta,
  MovimientoInventarioFiltros
} from '../../../core/models/inventario-bodega/dashboard-bodega/movimientos-inventario.model';

import { MovimientosInventarioService } from '../../../core/services/inventario-bodega/dashboard-bodega/movimientos-inventario.service';

@Component({
  selector: 'app-dashboard-bodega',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './dashboard-bodega.html',
  styleUrl: './dashboard-bodega.scss',
})
export class DashboardBodega implements OnInit {

  auditoria: MovimientoInventarioAuditoriaCompleta | null = null;

  movimientos: MovimientoInventarioAuditoria[] = [];

  fechaInicio = '';
  fechaFin = '';
  filtroTipo = '';
  buscar = '';

  buscarUsuario = '';
  buscarCliente = '';

  tiposMovimiento: string[] = [];
  usuariosDisponibles: string[] = [];
  clientesDisponibles: string[] = [];

  cargando = false;
  error = '';

  // Paginación controlada por backend
  paginaActual = 1;
  elementosPorPagina = 10;

  constructor(
    private movimientosService: MovimientosInventarioService
  ) { }

  ngOnInit(): void {
    this.cargarOpcionesFiltros();
    this.cargarAuditoria();
  }

  // Cargar auditoría con filtros y paginación desde backend
  cargarAuditoria(): void {
    this.cargando = true;
    this.error = '';

    const filtros: MovimientoInventarioFiltros = {
      fechaInicio: this.fechaInicio || undefined,
      fechaFin: this.fechaFin || undefined,
      tipo: this.filtroTipo || undefined,
      referencia: this.buscar.trim() || undefined,
      usuario: this.buscarUsuario || undefined,
      cliente: this.buscarCliente || undefined,
      pagina: this.paginaActual,
      pageSize: this.elementosPorPagina
    };

    this.movimientosService.obtenerAuditoria(filtros).subscribe({
      next: (respuesta) => {
        this.auditoria = respuesta;
        this.movimientos = respuesta.movimientos || [];

        // Sincronizar paginación con la respuesta del backend
        this.paginaActual = respuesta.pagina;
        this.elementosPorPagina = respuesta.pageSize;

        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar la auditoría:', error);
        this.error = 'No fue posible cargar los movimientos de inventario.';
        this.cargando = false;
      }
    });
  }

  cargarOpcionesFiltros(): void {
    this.movimientosService.obtenerOpcionesFiltros().subscribe({
      next: (respuesta) => {
        this.tiposMovimiento = respuesta.tipos || [];
        this.usuariosDisponibles = respuesta.usuarios || [];
        this.clientesDisponibles = respuesta.clientes || [];
      },
      error: (error) => {
        console.error('Error al cargar opciones de filtros:', error);
      }
    });
  }

  // Aplicar filtros
  aplicarFiltros(): void {
    this.paginaActual = 1;
    this.cargarAuditoria();
  }

  // Limpiar filtros
  limpiarFiltros(): void {
    this.fechaInicio = '';
    this.fechaFin = '';
    this.filtroTipo = '';
    this.buscar = '';
    this.buscarUsuario = '';
    this.buscarCliente = '';

    this.paginaActual = 1;

    this.cargarAuditoria();
  }

  // Total de registros según backend
  get totalRegistros(): number {
    return this.auditoria?.totalRegistros || 0;
  }

  // Total de páginas según backend
  get totalPaginas(): number {
    return this.auditoria?.totalPaginas || 1;
  }

  // Cambiar página consultando nuevamente el backend
  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas || pagina === this.paginaActual) {
      return;
    }

    this.paginaActual = pagina;
    this.cargarAuditoria();
  }

  // Cambiar cantidad de registros por página
  cambiarCantidadPorPagina(cantidad: number): void {
    this.elementosPorPagina = cantidad;
    this.paginaActual = 1;
    this.cargarAuditoria();
  }

  // Números de páginas visibles
  get paginas(): number[] {
    const total = this.totalPaginas;
    const actual = this.paginaActual;

    const inicio = Math.max(1, actual - 2);
    const fin = Math.min(total, inicio + 4);
    const inicioAjustado = Math.max(1, fin - 4);

    return Array.from(
      { length: fin - inicioAjustado + 1 },
      (_, i) => inicioAjustado + i
    );
  }

  obtenerClaseTipo(tipo: string): string {
    switch (tipo?.toLowerCase()) {
      case 'inicial':
        return 'tipo-inicial';

      case 'entrada':
        return 'tipo-entrada';

      case 'devolucion':
        return 'tipo-devolucion';

      case 'salida':
        return 'tipo-salida';

      case 'eliminado':
        return 'tipo-eliminado';

      case 'ajuste':
        return 'tipo-ajuste';

      default:
        return 'tipo-default';
    }
  }

  obtenerNombreTipo(tipo: string): string {
    switch (tipo?.toLowerCase()) {
      case 'inicial': return 'Inventario inicial';
      case 'entrada': return 'Entrada';
      case 'devolucion': return 'Devolución';
      case 'salida': return 'Salida';
      case 'eliminado': return 'Eliminado';
      case 'ajuste': return 'Ajuste';
      default: return tipo || 'Sin tipo';
    }
  }

  formatearPeso(peso: number): string {
    return (peso || 0).toLocaleString('es-CO', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  }

  // Exportar los movimientos de la página actual
  exportarCSV(): void {
    if (!this.movimientos.length) {
      return;
    }

    const encabezados = [
      'Referencia',
      'Referencia peso',
      'Tipo',
      'Peso',
      'Fecha',
      'Usuario',
      'Cliente',
      'Referencia impresa',
      'Observación'
    ];

    const filas = this.movimientos.map(m => [
      m.referencia,
      m.referenciaPeso,
      this.obtenerNombreTipo(m.tipo),
      m.peso,
      m.fecha,
      m.usuario,
      m.cliente || '',
      m.referenciaImpresa || '',
      m.observacion || ''
    ]);

    const contenido = [
      encabezados,
      ...filas
    ].map(fila =>
      fila.map(valor =>
        `"${String(valor ?? '').replace(/"/g, '""')}"`
      ).join(';')
    ).join('\n');

    const blob = new Blob(
      ['\uFEFF' + contenido],
      { type: 'text/csv;charset=utf-8;' }
    );

    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');

    enlace.href = url;
    enlace.download = 'movimientos-inventario.csv';
    enlace.click();

    URL.revokeObjectURL(url);
  }

  // Exportar PDF
  exportarPDF(): void {

    const filtros: MovimientoInventarioFiltros = {
      fechaInicio: this.fechaInicio || undefined,
      fechaFin: this.fechaFin || undefined,
      tipo: this.filtroTipo || undefined,
      referencia: this.buscar.trim() || undefined,
      usuario: this.buscarUsuario.trim() || undefined,
      cliente: this.buscarCliente.trim() || undefined
    };

    // Abrir ventana inmediatamente para evitar bloqueo del navegador
    const ventana = window.open('', '_blank');

    if (!ventana) {
      this.error = 'El navegador bloqueó la ventana emergente. Permite las ventanas emergentes para generar el PDF.';
      return;
    }

    this.cargando = true;
    this.error = '';

    this.movimientosService.generarPdf(filtros).subscribe({

      next: (respuesta: Blob) => {

        const archivo = new Blob(
          [respuesta],
          { type: 'application/pdf' }
        );

        const url = window.URL.createObjectURL(archivo);

        ventana.location.href = url;

        // Liberar memoria después de un tiempo
        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 60000);

        this.cargando = false;
      },

      error: (error) => {

        console.error('Error al generar PDF:', error);

        ventana.close();

        this.error = 'No fue posible generar el PDF de movimientos.';

        this.cargando = false;
      }

    });
  }

}
