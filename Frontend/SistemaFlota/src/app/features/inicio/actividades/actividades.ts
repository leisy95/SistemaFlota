import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { DashboardResumenInicio } from '../../../core/models/Dashboards/dashboard-resumen-inicio/dashboard-resumen-inicio.model';

import { DashboardResumenInicioService } from '../../../core/services/dashboards/dashboard-resumen-inicio/dashboard-resumen-inicio.service';

@Component({
  selector: 'app-actividades',
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './actividades.html',
  styleUrl: './actividades.scss',
})
export class Actividades implements OnInit {

  resumen: DashboardResumenInicio | null = null;

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
          'Error al cargar las actividades del dashboard:',
          error
        );
      }
    });

  }
}