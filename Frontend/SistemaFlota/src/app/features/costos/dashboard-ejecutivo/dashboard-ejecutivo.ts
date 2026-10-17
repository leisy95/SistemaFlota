import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import {
  Chart,
  ChartData,
  ChartOptions,
  registerables
} from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardEjecutivoDto } from '../../../core/models/costos/DashboardEjecutivo/dashboard-ejecutivo.model'; import { FormsModule } from '@angular/forms';
import { DashboardEjecutivoService, FiltrosDashboardEjecutivo, OpcionesFiltrosDashboard } from '../../../core/services/costos/DashboardEjecutivo/dashboardejecutivo.service ';
import { DashboardFiltros } from './componentes/dashboard-filtros/dashboard-filtros';
import { DashboardIndicadores } from './componentes/dashboard-indicadores/dashboard-indicadores';
import { DashboardGraficas } from './componentes/dashboard-graficas/dashboard-graficas';
import { DashboardMateriales } from './componentes/dashboard-materiales/dashboard-materiales';
import { DashboardProveedores } from './componentes/dashboard-proveedores/dashboard-proveedores';

Chart.register(...registerables);

interface StatCard {
  title: string;
  value: string;
  subtitle: string;
  extra: string;
  iconClass: string;
  iconType: string;
}

interface SupplierSummary {
  proveedor: string;
  kgTotal: number;
  bultos: number;
  valorTotal: number;
  porcentajeInventario: number;
}

@Component({
  selector: 'app-dashboard-ejecutivo',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DashboardFiltros,
    DashboardIndicadores,
    DashboardGraficas,
    DashboardMateriales,
    DashboardProveedores
  ],
  templateUrl: './dashboard-ejecutivo.html',
  styleUrl: './dashboard-ejecutivo.scss'
})
export class DashboardEjecutivo implements OnInit {

  private readonly dashboardService = inject(DashboardEjecutivoService);

  // ESTADO DEL COMPONENTE
  cargando = false;
  errorCarga = '';

  dashboard: DashboardEjecutivoDto | null = null;


  // FILTROS DEL DASHBOARD

  filtrosFormulario: FiltrosDashboardEjecutivo = {
    fechaDesde: '',
    fechaHasta: '',
    proveedor: '',
    estado: '',
    categoria: '',
    densidad: ''
  };

  filtrosAplicados: FiltrosDashboardEjecutivo = {};

  estados: string[] = [];
  categorias: string[] = [];
  densidades: string[] = [];
  proveedores: string[] = [];

  aplicandoFiltros = false;

  mensajeFiltros = '';

  // APLICAR FILTROS

  aplicarFiltros(): void {
    if (
      this.filtrosFormulario.fechaDesde &&
      this.filtrosFormulario.fechaHasta &&
      this.filtrosFormulario.fechaDesde >
      this.filtrosFormulario.fechaHasta
    ) {
      this.mensajeFiltros =
        'La fecha inicial no puede ser posterior a la fecha final.';
      return;
    }

    this.mensajeFiltros = '';

    this.filtrosAplicados = {
      fechaDesde: this.filtrosFormulario.fechaDesde || undefined,
      fechaHasta: this.filtrosFormulario.fechaHasta || undefined,
      proveedor: this.filtrosFormulario.proveedor?.trim() || undefined,
      estado: this.filtrosFormulario.estado?.trim() || undefined,
      categoria: this.filtrosFormulario.categoria?.trim() || undefined,
      densidad: this.filtrosFormulario.densidad?.trim() || undefined
    };

    this.paginaMateriales = 1;
    this.paginaProveedores = 1;

    this.cargarDashboard();
  }

  // CARGAR OPCIONES DE LOS FILTROS

  private cargarFiltros(): void {
    this.dashboardService.obtenerFiltros().subscribe({
      next: (respuesta: OpcionesFiltrosDashboard) => {
        this.estados = respuesta.estados ?? [];
        this.categorias = respuesta.categorias ?? [];
        this.densidades = respuesta.densidades ?? [];
        this.proveedores = respuesta.proveedores ?? [];
      },
      error: (error) => {
        console.error('Error al cargar las opciones de filtros:', error);
      }
    });
  }

  // LIMPIAR FILTROS

  limpiarFiltros(): void {
    this.filtrosFormulario = {
      fechaDesde: '',
      fechaHasta: '',
      proveedor: '',
      estado: '',
      categoria: '',
      densidad: ''
    };

    this.filtrosAplicados = {};
    this.mensajeFiltros = '';
    this.paginaMateriales = 1;
    this.paginaProveedores = 1;

    this.cargarDashboard();
  }

  // TARJETAS ESTADÍSTICAS
  statsCards: StatCard[] = [];

  // RESUMEN DE PROVEEDORES
  supplierSummaryList: SupplierSummary[] = [];


  // PAGINACIÓN DE TABLAS
  readonly registrosPorPagina = 5;

  paginaMateriales = 1;
  paginaProveedores = 1;
  cargandoMateriales = false;

  // MATERIALES CON MAYOR VALOR DE COMPRA
  get materialesPaginados() {
    return this.dashboard?.materialesMayorCosto ?? [];
  }

  get totalPaginasMateriales(): number {
    const total = this.dashboard?.totalMaterialesMayorCosto ?? 0;
    const porPagina =
      this.dashboard?.porPaginaMateriales ?? this.registrosPorPagina;

    return Math.max(1, Math.ceil(total / porPagina));
  }

  cambiarPaginaMateriales(direccion: number): void {
    const nuevaPagina = this.paginaMateriales + direccion;

    if (
      nuevaPagina < 1 ||
      nuevaPagina > this.totalPaginasMateriales ||
      this.cargandoMateriales
    ) {
      return;
    }

    this.cargarPaginaMateriales(nuevaPagina);
  }
  private cargarPaginaMateriales(pagina: number): void {
    this.cargandoMateriales = true;

    this.dashboardService
      .obtenerDashboard(
        pagina,
        this.registrosPorPagina,
        this.filtrosAplicados
      )
      .subscribe({
        next: (respuesta: DashboardEjecutivoDto) => {
          this.dashboard = {
            ...this.dashboard!,
            ...respuesta
          };

          this.paginaMateriales =
            respuesta.paginaMateriales ?? pagina;

          this.cargandoMateriales = false;

          console.log('Página recibida:', this.paginaMateriales);
          console.log(
            'Total de materiales:',
            respuesta.totalMaterialesMayorCosto
          );
          console.log(
            'Materiales recibidos:',
            respuesta.materialesMayorCosto.length
          );
        },
        error: (error) => {
          console.error(
            'Error al cambiar la página de materiales:',
            error
          );

          this.cargandoMateriales = false;
        }
      });
  }

  // RESUMEN POR PROVEEDOR

  get proveedoresPaginados(): SupplierSummary[] {
    const inicio =
      (this.paginaProveedores - 1) * this.registrosPorPagina;

    return this.supplierSummaryList.slice(
      inicio,
      inicio + this.registrosPorPagina
    );
  }

  get totalPaginasProveedores(): number {
    return Math.max(
      1,
      Math.ceil(
        this.supplierSummaryList.length / this.registrosPorPagina
      )
    );
  }

  cambiarPaginaProveedores(direccion: number): void {
    this.paginaProveedores = Math.max(
      1,
      Math.min(
        this.totalPaginasProveedores,
        this.paginaProveedores + direccion
      )
    );
  }

  // GRÁFICA 1: STOCK POR PROVEEDOR

  stockProveedorData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Stock (kg)',
        data: [],
        backgroundColor: '#0d9488',
        borderRadius: 6,
        barThickness: 22
      }
    ]
  };

  stockProveedorOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: context =>
            `${this.formatearNumero(context.parsed.x ?? 0, 2)} kg`
        }
      }
    },
    scales: {
      x: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Kilogramos'
        }
      }
    }
  };

  // GRÁFICA 2: DISTRIBUCIÓN POR CATEGORÍA

  tipoMaterialData: ChartData<'pie'> = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [
          '#6366f1',
          '#14b8a6',
          '#f59e0b',
          '#94a3b8',
          '#ec4899',
          '#8b5cf6',
          '#0ea5e9',
          '#84cc16'
        ],
        borderWidth: 2,
        borderColor: '#ffffff'
      }
    ]
  };

  // OPCIONES DE GRÁFICAS CIRCULARES

  pastelOptions: ChartOptions<'pie'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom'
      },
      tooltip: {
        callbacks: {
          label: context => {
            const valores = context.dataset.data as number[];

            const total = valores.reduce(
              (suma, valor) => suma + Number(valor ?? 0),
              0
            );

            const valor = Number(context.raw ?? 0);
            const porcentaje = total > 0
              ? (valor / total) * 100
              : 0;

            return `${context.label}: ${this.formatearNumero(valor)} (${porcentaje.toFixed(1)}%)`;
          }
        }
      }
    }
  };

  // GRÁFICA 3: DISTRIBUCIÓN POR DENSIDAD

  densidadData: ChartData<'pie'> = {
    labels: [],
    datasets: [
      {
        data: [],
        backgroundColor: [
          '#f97316',
          '#0d9488',
          '#8b5cf6',
          '#3b82f6',
          '#eab308',
          '#ec4899',
          '#64748b',
          '#22c55e'
        ],
        borderWidth: 2,
        borderColor: '#ffffff'
      }
    ]
  };

  // GRÁFICA 4: VALOR DEL INVENTARIO POR PROVEEDOR

  valorProveedorData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Valor del inventario',
        data: [],
        backgroundColor: '#6366f1',
        borderRadius: 6,
        barThickness: 22
      }
    ]
  };

  valorProveedorOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    indexAxis: 'y',
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        callbacks: {
          label: context =>
            this.formatearMoneda(context.parsed.x ?? 0)
        }
      }
    },
    scales: {
      x: {
        beginAtZero: true,
        ticks: {
          callback: value =>
            new Intl.NumberFormat('es-CO', {
              notation: 'compact',
              maximumFractionDigits: 1
            }).format(Number(value))
        }
      }
    }
  };

  // GRÁFICA 5: EVOLUCIÓN MENSUAL DE COMPRAS


  // GRÁFICA 5: EVOLUCIÓN MENSUAL DE COMPRAS

  comprasPorMesData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        label: 'Valor de compras',
        data: [],
        borderColor: '#6366f1',
        backgroundColor: 'rgba(99, 102, 241, 0.18)',
        pointBackgroundColor: '#ffffff',
        pointBorderColor: '#6366f1',
        pointRadius: 5,
        pointHoverRadius: 8,
        pointBorderWidth: 3,
        borderWidth: 3,
        tension: 0.4,
        fill: true,
        yAxisID: 'y'
      },
      {
        label: 'Kilogramos comprados',
        data: [],
        borderColor: '#0d9488',
        backgroundColor: 'rgba(13, 148, 136, 0.15)',
        pointBackgroundColor: '#ffffff',
        pointBorderColor: '#0d9488',
        pointRadius: 5,
        pointHoverRadius: 8,
        pointBorderWidth: 3,
        borderWidth: 3,
        tension: 0.4,
        fill: true,
        yAxisID: 'y1'
      }
    ]
  };

  comprasPorMesOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,

    interaction: {
      intersect: false,
      mode: 'index'
    },

    plugins: {
      legend: {
        display: true,
        position: 'top'
      },
      tooltip: {
        enabled: true,
        backgroundColor: '#1e293b',
        titleColor: '#ffffff',
        bodyColor: '#ffffff',
        padding: 12,
        cornerRadius: 10
      }
    },

    scales: {
      x: {
        grid: {
          display: false
        },
        border: {
          display: false
        }
      },

      y: {
        type: 'linear',
        position: 'left',
        beginAtZero: true,
        title: {
          display: true,
          text: 'Valor de compras (COP)'
        },
        border: {
          display: false
        },
        grid: {
          color: 'rgba(148, 163, 184, 0.15)'
        },
        ticks: {
          callback: (value) =>
            new Intl.NumberFormat('es-CO', {
              notation: 'compact',
              maximumFractionDigits: 1
            }).format(Number(value))
        }
      },

      y1: {
        type: 'linear',
        position: 'right',
        beginAtZero: true,
        title: {
          display: true,
          text: 'Kilogramos (kg)'
        },
        border: {
          display: false
        },
        grid: {
          drawOnChartArea: false
        }
      }
    }
  };

  // INICIALIZACIÓN

  ngOnInit(): void {
    this.cargarFiltros();
    this.cargarDashboard();
  }

  // CONSULTAR DATOS DEL BACKEND

  cargarDashboard(): void {
    this.cargando = true;
    this.errorCarga = '';

    this.dashboardService
      .obtenerDashboard(
        1,
        this.registrosPorPagina,
        this.filtrosAplicados
      )
      .subscribe({
        next: (respuesta: DashboardEjecutivoDto) => {
          this.dashboard = respuesta;

          this.paginaMateriales =
            respuesta.paginaMateriales ?? 1;

          this.paginaProveedores = 1;

          this.actualizarResumenProveedores();
          this.actualizarGraficas();
          this.actualizarTarjetas();

          this.cargando = false;
        },
        error: (error) => {
          console.error(
            'Error al cargar el Dashboard Ejecutivo:',
            error
          );

          this.errorCarga =
            error?.status === 403
              ? 'No tienes permisos para consultar el dashboard.'
              : error?.status === 401
                ? 'Tu sesión ha expirado. Inicia sesión nuevamente.'
                : 'No fue posible cargar la información del dashboard.';

          this.cargando = false;
        }
      });
  }

  // FORMATEAR NÚMEROS

  private formatearNumero(
    valor: number,
    decimales = 0
  ): string {
    return new Intl.NumberFormat('es-CO', {
      minimumFractionDigits: decimales,
      maximumFractionDigits: decimales
    }).format(valor ?? 0);
  }

  // FORMATEAR MONEDA COLOMBIANA

  private formatearMoneda(
    valor: number,
    decimales = 0
  ): string {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: decimales,
      maximumFractionDigits: decimales
    }).format(valor ?? 0);
  }

  // TARJETAS CON INFORMACIÓN DEL BACKEND

  private actualizarTarjetas(): void {
    if (!this.dashboard) {
      this.statsCards = [];
      return;
    }

    const d = this.dashboard;

    this.statsCards = [
      {
        title: 'STOCK TOTAL',
        value: `${this.formatearNumero(d.totalKg)} kg`,
        subtitle: `${this.formatearNumero(d.totalBultos)} bultos`,
        extra: 'Existencias actuales',
        iconClass: 'fa-boxes-stacked',
        iconType: 'total'
      },
      {
        title: 'VALOR INVENTARIO',
        value: this.formatearMoneda(d.totalValor),
        subtitle: 'Valor total actual',
        extra: `Costo prom.: ${this.formatearMoneda(d.costoPromedio, 2)}/kg`,
        iconClass: 'fa-dollar-sign',
        iconType: 'valor'
      },
      {
        title: 'OC PENDIENTES',
        value: this.formatearNumero(d.ordenesPendientes),
        subtitle: `De ${this.formatearNumero(d.totalOrdenes)} órdenes`,
        extra: 'Órdenes registradas',
        iconClass: 'fa-clock',
        iconType: 'pendiente'
      },
      {
        title: 'PROVEEDORES',
        value: this.formatearNumero(d.totalProveedores),
        subtitle: 'Proveedores activos',
        extra: 'Base de proveedores',
        iconClass: 'fa-users',
        iconType: 'proveedores'
      }
    ];
  }

  // ADAPTAR LOS DATOS DE PROVEEDORES A LA TABLA

  private actualizarResumenProveedores(): void {
    if (!this.dashboard) {
      this.supplierSummaryList = [];
      return;
    }

    const proveedores = this.dashboard.stockPorProveedor ?? [];

    this.supplierSummaryList = proveedores.map(proveedor => ({
      proveedor: proveedor.proveedor || 'Sin proveedor',
      kgTotal: proveedor.kg ?? 0,
      bultos: proveedor.bultos ?? 0,
      valorTotal: proveedor.valor ?? 0,
      porcentajeInventario: proveedor.porcentaje ?? 0
    }));
  }

  // ACTUALIZAR LAS CINCO GRÁFICAS

  private actualizarGraficas(): void {
    if (!this.dashboard) {
      return;
    }

    const d = this.dashboard;

    const stockPorProveedor = d.stockPorProveedor ?? [];
    const materialesPorCategoria = d.materialesPorCategoria ?? [];
    const materialesPorDensidad = d.materialesPorDensidad ?? [];
    const valorPorProveedor = d.valorPorProveedor ?? [];
    const comprasPorMes = d.comprasPorMes ?? [];

    // STOCK POR PROVEEDOR

    this.stockProveedorData = {
      labels: stockPorProveedor.map(
        p => p.proveedor || 'Sin proveedor'
      ),
      datasets: [
        {
          label: 'Stock (kg)',
          data: stockPorProveedor.map(p => p.kg ?? 0),
          backgroundColor: '#0d9488',
          borderRadius: 6,
          barThickness: 22
        }
      ]
    };

    // MATERIALES POR CATEGORÍA

    this.tipoMaterialData = {
      labels: materialesPorCategoria.map(
        item => item.categoria || 'Sin categoría'
      ),
      datasets: [
        {
          data: materialesPorCategoria.map(
            item => item.cantidad ?? 0
          ),
          backgroundColor: [
            '#6366f1',
            '#14b8a6',
            '#f59e0b',
            '#94a3b8',
            '#ec4899',
            '#8b5cf6',
            '#0ea5e9',
            '#84cc16'
          ],
          borderWidth: 2,
          borderColor: '#ffffff'
        }
      ]
    };

    // MATERIALES POR DENSIDAD

    this.densidadData = {
      labels: materialesPorDensidad.map(
        item => item.densidad || 'Sin densidad'
      ),
      datasets: [
        {
          data: materialesPorDensidad.map(
            item => item.cantidad ?? 0
          ),
          backgroundColor: [
            '#f97316',
            '#0d9488',
            '#8b5cf6',
            '#3b82f6',
            '#eab308',
            '#ec4899',
            '#64748b',
            '#22c55e'
          ],
          borderWidth: 2,
          borderColor: '#ffffff'
        }
      ]
    };

    // VALOR DEL INVENTARIO POR PROVEEDOR

    this.valorProveedorData = {
      labels: valorPorProveedor.map(
        p => p.proveedor || 'Sin proveedor'
      ),
      datasets: [
        {
          label: 'Valor del inventario',
          data: valorPorProveedor.map(p => p.valor ?? 0),
          backgroundColor: '#6366f1',
          borderRadius: 6,
          barThickness: 22
        }
      ]
    };

    // EVOLUCIÓN MENSUAL DE COMPRAS
    this.comprasPorMesData = {
      labels: comprasPorMes.map(item => item.mes),
      datasets: [
        {
          label: 'Valor de compras',
          data: comprasPorMes.map(item => item.valor ?? 0),
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59, 130, 246, 0.12)',
          pointBackgroundColor: '#ffffff',
          pointBorderColor: '#3b82f6',
          pointRadius: 6,
          pointHoverRadius: 8,
          pointBorderWidth: 3,
          borderWidth: 4,
          tension: 0.35,
          fill: true
        }
      ]
    };
  }

  // TOTALES GENERALES DESDE EL BACKEND

  get totalKg(): number {
    return this.dashboard?.totalKg ?? 0;
  }

  get totalBultos(): number {
    return this.dashboard?.totalBultos ?? 0;
  }

  get totalValor(): number {
    return this.dashboard?.totalValor ?? 0;
  }

  get totalProveedores(): number {
    return this.dashboard?.totalProveedores ?? 0;
  }

  get costoPromedio(): number {
    return this.dashboard?.costoPromedio ?? 0;
  }
}