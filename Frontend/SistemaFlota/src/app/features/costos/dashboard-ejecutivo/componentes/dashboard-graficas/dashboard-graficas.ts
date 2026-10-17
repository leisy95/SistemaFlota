import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ChartData, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';

@Component({
  selector: 'app-dashboard-graficas',
  standalone: true,
  imports: [
    CommonModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard-graficas.html',
  styleUrl: './dashboard-graficas.scss',
})
export class DashboardGraficas {

  @Input() stockProveedorData!: ChartData<'bar'>;
  @Input() stockProveedorOptions!: ChartOptions<'bar'>;

  @Input() tipoMaterialData!: ChartData<'pie'>;
  @Input() densidadData!: ChartData<'pie'>;
  @Input() pastelOptions!: ChartOptions<'pie'>;

  @Input() valorProveedorData!: ChartData<'bar'>;
  @Input() valorProveedorOptions!: ChartOptions<'bar'>;

  @Input() comprasPorMesData!: ChartData<'line'>;

  @Input() comprasPorMesOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    layout: {
      padding: {
        left: 10,
        right: 10,
        top: 10,
        bottom: 0
      }
    },
    plugins: {
      legend: {
        display: false
      },
      tooltip: {
        enabled: true
      }
    },
    scales: {
      x: {
        grid: {
          display: false
        },
        border: {
          display: false
        },
        ticks: {
          color: '#64748b',
          font: {
            size: 14,
            family: 'system-ui, -apple-system, sans-serif'
          },
          padding: 12
        }
      },
      y: {
        display: true,
        beginAtZero: true,
        border: {
          display: false
        },
        grid: {
          color: '#e2e8f0',
          drawTicks: false
        },
        ticks: {
          display: false
        }
      }
    }
  };

  @Input() hayComprasMensuales = false;
}