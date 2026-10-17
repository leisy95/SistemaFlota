import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';


export interface StatCard {
  title: string;
  value: string;
  subtitle: string;
  extra: string;
  iconType: string;
  iconClass: string;
}

@Component({
  selector: 'app-dashboard-indicadores',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './dashboard-indicadores.html',
  styleUrl: './dashboard-indicadores.scss',
})
export class DashboardIndicadores {

  @Input() statsCards: StatCard[] = [];

  @Input() gastoCompras = 0;
  @Input() recepcionesPendientes = 0;
  @Input() totalRecepciones = 0;
  @Input() kgRecibidos = 0;
  @Input() bultosRecibidos = 0;
  @Input() entregasPendientesInventario = 0;
}
