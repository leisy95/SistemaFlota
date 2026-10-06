import { Component, OnInit } from '@angular/core';

import { DashboardResumenInicio } from '../../../core/models/Dashboards/dashboard-resumen-inicio/dashboard-resumen-inicio.model';

import { DashboardResumenInicioService } from '../../../core/services/dashboards/dashboard-resumen-inicio/dashboard-resumen-inicio.service';

@Component({
  selector: 'app-resumen',
  imports: [],
  templateUrl: './resumen.html',
  styleUrl: './resumen.scss',
})
export class Resumen implements OnInit {

  resumen: DashboardResumenInicio = {
    vehiculos: 0,
    empleados: 0,
    ordenesCompra: 0,
    ordenesPendientes: 0,
    mantenimientos: 0,
    mantenimientosProximos: 0,
    actividades: [],
    vencimientos: []
  };

  constructor(
    private dashboardResumenService: DashboardResumenInicioService
  ) { }

  ngOnInit(): void {
    this.obtenerResumen();
  }

  obtenerResumen(): void {

    this.dashboardResumenService.obtenerResumen().subscribe({
      next: (respuesta) => {
        this.resumen = respuesta;
      },
      error: (error) => {
        console.error(
          'Error al cargar el resumen del dashboard:',
          error
        );
      }
    });

  }
}