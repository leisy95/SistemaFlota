import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

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
    CommonModule
  ],
  templateUrl: './dashboard-ejecutivo.html',
  styleUrl: './dashboard-ejecutivo.scss',
})
export class DashboardEjecutivo {
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

  get totalKg(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.kgTotal, 0
    );
  }

  get totalBultos(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.bultos, 0
    );
  }

  get totalValor(): number {
    return this.supplierSummaryList.reduce(
      (total, item) => total + item.valorTotal, 0
    );
  }

  get totalProveedores(): number {
    return this.supplierSummaryList.length;
  }

  get costoPromedio(): number {
    return this.totalKg > 0 ? this.totalValor / this.totalKg : 0;
  }
}
