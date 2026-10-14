import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  Chart,
  ChartData,
  ChartOptions,
  registerables
} from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

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
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './dashboard-ejecutivo.html',
  styleUrl: './dashboard-ejecutivo.scss',
})
export class DashboardEjecutivo {

  // TARJETAS ESTADÍSTICAS

  statsCards: StatCard[] = [
    {
      title: 'STOCK TOTAL',
      value: '1.790 kg',
      subtitle: '8 bultos',
      extra: '7 materiales',
      iconClass: 'fa-boxes-stacked',
      iconType: 'total'
    },
    {
      title: 'VALOR INVENTARIO',
      value: '$93.000',
      subtitle: 'Valor total actual',
      extra: 'Costo prom.: $51,96/kg',
      iconClass: 'fa-dollar-sign',
      iconType: 'valor'
    },
    {
      title: 'OC PENDIENTES',
      value: '2',
      subtitle: 'De 3 órdenes',
      extra: 'Órdenes activas',
      iconClass: 'fa-clock',
      iconType: 'pendiente'
    },
    {
      title: 'PROVEEDORES',
      value: '6',
      subtitle: 'Proveedores registrados',
      extra: 'Base de proveedores',
      iconClass: 'fa-users',
      iconType: 'proveedores'
    }
  ];

  // RESUMEN DE PROVEEDORES

  supplierSummaryList: SupplierSummary[] = [
    {
      proveedor: 'PLASTICOS Y PIGMENTOS',
      kgTotal: 50,
      bultos: 2,
      valorTotal: 20000,
      porcentajeInventario: 2.8
    },
    {
      proveedor: 'NACIONAL',
      kgTotal: 20,
      bultos: 1,
      valorTotal: 5000,
      porcentajeInventario: 1.1
    },
    {
      proveedor: 'CPA',
      kgTotal: 520,
      bultos: 2,
      valorTotal: 9000,
      porcentajeInventario: 29.1
    },
    {
      proveedor: 'POLYREC',
      kgTotal: 500,
      bultos: 1,
      valorTotal: 4000,
      porcentajeInventario: 27.9
    },
    {
      proveedor: 'IA4',
      kgTotal: 500,
      bultos: 1,
      valorTotal: 5000,
      porcentajeInventario: 27.9
    },
    {
      proveedor: 'NN',
      kgTotal: 200,
      bultos: 1,
      valorTotal: 50000,
      porcentajeInventario: 11.2
    }
  ];

  // GRÁFICA 1: STOCK POR PROVEEDOR (KG)

  stockProveedorData: ChartData<'bar'> = {
    labels: this.supplierSummaryList.map(p => p.proveedor),
    datasets: [
      {
        label: 'Stock (kg)',
        data: this.supplierSummaryList.map(p => p.kgTotal),
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
          label: context => `${context.parsed.x} kg`
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

  // GRÁFICA 2: DISTRIBUCIÓN POR TIPO DE MATERIAL
  // DATOS ILUSTRATIVOS: sustituir por inventario real

  tipoMaterialData: ChartData<'pie'> = {
    labels: [
      'Polietileno',
      'Polipropileno',
      'PET',
      'Otros'
    ],
    datasets: [
      {
        data: [40, 30, 20, 10],
        backgroundColor: [
          '#6366f1',
          '#14b8a6',
          '#f59e0b',
          '#94a3b8'
        ],
        borderWidth: 2,
        borderColor: '#ffffff'
      }
    ]
  };

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
            const values = context.dataset.data as number[];
            const total = values.reduce(
              (sum, value) => sum + value,
              0
            );

            const value = Number(context.raw);
            const porcentaje = total > 0
              ? (value / total) * 100
              : 0;

            return `${context.label}: ${porcentaje.toFixed(1)}%`;
          }
        }
      }
    }
  };

  // GRÁFICA 3: DISTRIBUCIÓN POR DENSIDAD
  // DATOS ILUSTRATIVOS: sustituir por inventario real

  densidadData: ChartData<'pie'> = {
    labels: [
      'Baja',
      'Media',
      'Alta'
    ],
    datasets: [
      {
        data: [35, 45, 20],
        backgroundColor: [
          '#f97316',
          '#0d9488',
          '#8b5cf6'
        ],
        borderWidth: 2,
        borderColor: '#ffffff'
      }
    ]
  };

  //  GRÁFICA 4: VALOR TOTAL POR PROVEEDOR

  valorProveedorData: ChartData<'bar'> = {
    labels: this.supplierSummaryList.map(p => p.proveedor),
    datasets: [
      {
        label: 'Valor del inventario',
        data: this.supplierSummaryList.map(p => p.valorTotal),
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
            new Intl.NumberFormat('es-CO', {
              style: 'currency',
              currency: 'COP',
              maximumFractionDigits: 0
            }).format(context.parsed.x ?? 0)
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

  // TOTALES DEL DASHBOARD

  get totalKg(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.kgTotal,
      0
    );
  }

  get totalBultos(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.bultos,
      0
    );
  }

  get totalValor(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.valorTotal,
      0
    );
  }

  get totalProveedores(): number {
    return this.supplierSummaryList.length;
  }

  get costoPromedio(): number {
    return this.totalKg > 0
      ? this.totalValor / this.totalKg
      : 0;
  }
}