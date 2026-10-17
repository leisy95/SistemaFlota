import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FiltrosDashboardEjecutivo } from '../../../../../core/services/costos/DashboardEjecutivo/dashboardejecutivo.service ';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard-filtros',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './dashboard-filtros.html',
  styleUrl: './dashboard-filtros.scss',
})
export class DashboardFiltros {

  @Input({ required: true }) filtros!: FiltrosDashboardEjecutivo;

  @Input() estados: string[] = [];
  @Input() categorias: string[] = [];
  @Input() densidades: string[] = [];
  @Input() proveedores: string[] = [];

  @Input() mensaje = '';
  @Input() aplicando = false;

  @Output() aplicar = new EventEmitter<void>();
  @Output() limpiar = new EventEmitter<void>();
}
