import { Component, OnInit } from '@angular/core';
import { MovimientoInventarioAuditoria, MovimientoInventarioAuditoriaCompleta } from '../../../core/models/inventario-bodega/dashboard-bodega/movimientos-inventario.model';
import { MovimientosInventarioService } from '../../../core/services/inventario-bodega/dashboard-bodega/movimientos-inventario.service';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

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
  movimientosFiltrados: MovimientoInventarioAuditoria[] = [];

  fechaInicio = '';
  fechaFin = '';
  filtroTipo = '';
  buscar = '';

  cargando = false;
  error = '';

  paginaActual = 1;
  elementosPorPagina = 10;

  constructor(
    private movimientosService: MovimientosInventarioService
  ) { }

  ngOnInit(): void {
    this.cargarAuditoria();
  }

  cargarAuditoria(): void {
    this.cargando = true;
    this.error = '';

    this.movimientosService.obtenerAuditoria(
      this.fechaInicio || undefined,
      this.fechaFin || undefined
    ).subscribe({
      next: (respuesta) => {
        this.auditoria = respuesta;
        this.movimientos = respuesta.movimientos || [];
        this.aplicarFiltros();
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar la auditoría:', error);
        this.error = 'No fue posible cargar los movimientos de inventario.';
        this.cargando = false;
      }
    });
  }

  aplicarFiltros(): void {
    const termino = this.buscar.trim().toLowerCase();

    this.movimientosFiltrados = this.movimientos.filter(movimiento => {
      const coincideTipo = !this.filtroTipo ||
        movimiento.tipo?.toLowerCase() === this.filtroTipo.toLowerCase();

      const coincideTexto = !termino ||
        movimiento.referencia?.toLowerCase().includes(termino) ||
        movimiento.referenciaPeso?.toLowerCase().includes(termino) ||
        movimiento.usuario?.toLowerCase().includes(termino) ||
        movimiento.cliente?.toLowerCase().includes(termino) ||
        movimiento.referenciaImpresa?.toLowerCase().includes(termino);

      return coincideTipo && coincideTexto;
    });

    this.paginaActual = 1;
  }

  limpiarFiltros(): void {
    this.fechaInicio = '';
    this.fechaFin = '';
    this.filtroTipo = '';
    this.buscar = '';

    this.cargarAuditoria();
  }

  get movimientosPaginados(): MovimientoInventarioAuditoria[] {
    const inicio = (this.paginaActual - 1) * this.elementosPorPagina;

    return this.movimientosFiltrados.slice(
      inicio,
      inicio + this.elementosPorPagina
    );
  }

  get totalPaginas(): number {
    return Math.ceil(
      this.movimientosFiltrados.length / this.elementosPorPagina
    ) || 1;
  }

  cambiarPagina(pagina: number): void {
    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.paginaActual = pagina;
  }

  cambiarCantidadPorPagina(cantidad: number): void {
    this.elementosPorPagina = cantidad;
    this.paginaActual = 1;
  }

  get paginas(): number[] {
    return Array.from(
      { length: this.totalPaginas },
      (_, i) => i + 1
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

  exportarCSV(): void {
    if (!this.movimientosFiltrados.length) {
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

    const filas = this.movimientosFiltrados.map(m => [
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
}
