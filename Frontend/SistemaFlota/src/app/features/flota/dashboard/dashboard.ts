import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Chart } from 'chart.js/auto';

import { ConductoresService } from '../../../core/services/conductores.service';
import { VehiculosService } from '../../../core/services/vehiculos.service';
import { InspeccionesService } from '../../../core/services/inspecciones.service';
import { AutorizacionesService } from '../../../core/services/autorizaciones.service';
import { IncidentesService } from '../../../core/services/incidentes.service';
import { MantenimientoService } from '../../../core/services/mantenimiento.service';
import { DocumentosService } from '../../../core/services/documentos.service';
import { PermisosService } from '../../../core/services/permisos.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {

  @ViewChild('graficoInspecciones')
  canvasInspecciones!: ElementRef<HTMLCanvasElement>;

  @ViewChild('graficoDona')
  canvasDona!: ElementRef<HTMLCanvasElement>;

  @Output() logout = new EventEmitter<void>();
  @Output() navegar = new EventEmitter<string>();

  horaActual = '';
  fechaActual = '';
  rolUsuario = '';

  private intervalo: ReturnType<typeof setInterval> | undefined;
  private graficoBarras?: Chart;
  private graficoDona?: Chart;

  totalConductores = 0;
  totalVehiculos = 0;
  totalInspecciones = 0;
  autorizacionesHoy = 0;

  documentosPorVencer = 0;
  vehiculosEnTaller = 0;
  incidentesPendientes = 0;

  ultimasInspecciones: any[] = [];
  ultimasAutorizaciones: any[] = [];

  autorizacionesPendientes = 0;
  autorizacionesAutorizadas = 0;
  autorizacionesRechazadas = 0;

  // Estos identificadores deben coincidir con permisos[].modulo del backend.
  private readonly todosModos = [
    { key: 'inspecciones', label: 'Inspecciones', icon: 'fa-solid fa-clipboard-check', color: 'blue' },
    { key: 'autorizaciones', label: 'Autorizaciones', icon: 'fa-solid fa-file-circle-check', color: 'green' },
    { key: 'reporte-ruta', label: 'Reporte Ruta', icon: 'fa-solid fa-triangle-exclamation', color: 'red' },
    { key: 'mantenimiento', label: 'Mantenimiento', icon: 'fa-solid fa-wrench', color: 'yellow' },
    { key: 'documentos', label: 'Documentos', icon: 'fa-solid fa-folder-open', color: 'purple' },
    { key: 'conductores', label: 'Conductores', icon: 'fa-solid fa-id-card', color: 'cyan' },
    { key: 'pedidos', label: 'Pedidos', icon: 'fa-solid fa-box', color: 'blue' },
    { key: 'cambio-ruta', label: 'Cambio Ruta', icon: 'fa-solid fa-route', color: 'green' },
    { key: 'solicitud-taller', label: 'Sol. Taller', icon: 'fa-solid fa-screwdriver-wrench', color: 'yellow' },
    { key: 'encuesta-fatiga', label: 'Enc. Fatiga', icon: 'fa-solid fa-face-tired', color: 'red' },
    { key: 'trazabilidad', label: 'Trazabilidad', icon: 'fa-solid fa-boxes-stacked', color: 'purple' },
    { key: 'incidentes', label: 'Incidentes', icon: 'fa-solid fa-car-burst', color: 'red' },
    { key: 'ver-inspecciones', label: 'Historial', icon: 'fa-solid fa-clock-rotate-left', color: 'cyan' }
  ];

  modulosRapidos: any[] = [];

  constructor(
    private conductoresService: ConductoresService,
    private vehiculosService: VehiculosService,
    private inspeccionesService: InspeccionesService,
    private autorizacionesService: AutorizacionesService,
    private incidentesService: IncidentesService,
    private mantenimientoService: MantenimientoService,
    private documentosService: DocumentosService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    const raw =
      sessionStorage.getItem('user') ||
      localStorage.getItem('user');

    if (raw) {
      try {
        this.rolUsuario = JSON.parse(raw)?.rol ?? '';
      } catch {
        this.rolUsuario = '';
      }
    }

    this.cargarModulosRapidos();
    this.actualizarHora();

    this.intervalo = setInterval(() => {
      this.actualizarHora();
    }, 1000);

    this.cargarDatos();

    setTimeout(() => {
      this.crearGraficas();
    }, 600);
  }

  ngOnDestroy(): void {
    if (this.intervalo) {
      clearInterval(this.intervalo);
    }

    this.graficoBarras?.destroy();
    this.graficoDona?.destroy();
  }

  // Muestra solo los módulos para los que puedeVer es true.
  cargarModulosRapidos(): void {
    this.modulosRapidos = this.todosModos.filter(
      modulo => this.permisosService.puedeVer(modulo.key)
    );
  }

  actualizarHora(): void {
    const ahora = new Date();

    this.horaActual = ahora.toLocaleTimeString('es-CO', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });

    this.fechaActual = ahora.toLocaleDateString('es-CO', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  cargarDatos(): void {

    // CONDUCTORES
    if (this.permisosService.puedeVer('conductores')) {
      this.conductoresService.obtenerTotalConductores().subscribe({
        next: (data: any) => {
          const arr = Array.isArray(data) ? data : (data?.data ?? []);
          this.totalConductores = arr.length;
        },
        error: err => console.error('Error al cargar conductores:', err)
      });
    } else {
      this.totalConductores = 0;
    }

    // VEHÍCULOS
    if (this.permisosService.puedeVer('vehiculos')) {
      this.vehiculosService.obtenerTotalVehiculos().subscribe({
        next: (data: any) => {
          const arr = Array.isArray(data) ? data : (data?.data ?? []);
          this.totalVehiculos = arr.length;
        },
        error: err => console.error('Error al cargar vehículos:', err)
      });
    } else {
      this.totalVehiculos = 0;
    }

    // INSPECCIONES
    if (this.permisosService.puedeVer('inspecciones')) {
      this.inspeccionesService.obtenerTotalInspecciones().subscribe({
        next: (data: any) => {
          const arr = Array.isArray(data) ? data : (data?.data ?? []);
          this.totalInspecciones = arr.length;
          this.ultimasInspecciones = arr.slice(0, 5);
        },
        error: err => console.error('Error al cargar inspecciones:', err)
      });
    } else {
      this.totalInspecciones = 0;
      this.ultimasInspecciones = [];
    }

    // AUTORIZACIONES
    if (this.permisosService.puedeVer('autorizaciones')) {
      this.autorizacionesService.obtenerAutorizaciones().subscribe({
        next: (res: any) => {
          const data: any[] = Array.isArray(res) ? res : (res?.data ?? []);
          const hoy = new Date().toDateString();

          this.autorizacionesHoy = data.filter(
            a => new Date(a.fechaCreacion).toDateString() === hoy
          ).length;

          this.autorizacionesPendientes = data.filter(
            a => a.estado === 'Pendiente'
          ).length;

          this.autorizacionesAutorizadas = data.filter(
            a => a.estado === 'Autorizado'
          ).length;

          this.autorizacionesRechazadas = data.filter(
            a => a.estado === 'Rechazado'
          ).length;

          this.ultimasAutorizaciones = data.slice(0, 5);
        },
        error: err => console.error('Error al cargar autorizaciones:', err)
      });
    } else {
      this.autorizacionesHoy = 0;
      this.autorizacionesPendientes = 0;
      this.autorizacionesAutorizadas = 0;
      this.autorizacionesRechazadas = 0;
      this.ultimasAutorizaciones = [];
    }

    // INCIDENTES
    if (this.permisosService.puedeVer('incidentes')) {
      this.incidentesService.obtenerIncidentes().subscribe({
        next: (data: any) => {
          const arr = Array.isArray(data) ? data : (data?.data ?? []);
          this.incidentesPendientes = arr.filter(
            (i: any) => i.estado === 'Pendiente'
          ).length;
        },
        error: err => console.error('Error al cargar incidentes:', err)
      });
    } else {
      this.incidentesPendientes = 0;
    }

    // MANTENIMIENTO
    if (this.permisosService.puedeVer('mantenimiento')) {
      this.mantenimientoService.obtenerTodos().subscribe({
        next: (data: any) => {
          const arr = Array.isArray(data) ? data : (data?.data ?? []);
          this.vehiculosEnTaller = arr.filter(
            (m: any) => m.estado === 'EnTaller'
          ).length;
        },
        error: err => console.error('Error al cargar mantenimiento:', err)
      });
    } else {
      this.vehiculosEnTaller = 0;
    }

    // DOCUMENTOS
    if (this.permisosService.puedeVer('documentos')) {
      this.documentosService.obtenerPorVencer().subscribe({
        next: (data: any) => {
          this.documentosPorVencer = data?.total ?? 0;
        },
        error: err => console.error('Error al cargar documentos:', err)
      });
    } else {
      this.documentosPorVencer = 0;
    }
  }

  crearGraficas(): void {
    this.crearGraficoBarras();
    this.crearGraficoDona();
  }

  crearGraficoBarras(): void {
    if (!this.canvasInspecciones?.nativeElement) {
      return;
    }

    this.graficoBarras?.destroy();

    this.graficoBarras = new Chart(
      this.canvasInspecciones.nativeElement,
      {
        type: 'bar',
        data: {
          labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
          datasets: [{
            label: 'Inspecciones',
            data: [5, 8, 6, 10, 7, 12],
            backgroundColor: '#90A4AE',
            borderColor: '#558B2F',
            borderWidth: 1,
            borderRadius: 8,
            borderSkipped: false
          }]
        },
        options: {
          responsive: true,
          plugins: {
            legend: { display: false }
          },
          scales: {
            y: {
              beginAtZero: true,
              grid: { color: 'rgba(255,255,255,0.05)' },
              ticks: { color: '#64748b' }
            },
            x: {
              grid: { display: false },
              ticks: { color: '#64748b' }
            }
          }
        }
      }
    );
  }

  crearGraficoDona(): void {
    if (!this.canvasDona?.nativeElement) {
      return;
    }

    this.graficoDona?.destroy();

    this.graficoDona = new Chart(
      this.canvasDona.nativeElement,
      {
        type: 'doughnut',
        data: {
          labels: ['Pendientes', 'Autorizadas', 'Rechazadas'],
          datasets: [{
            data: [
              this.autorizacionesPendientes,
              this.autorizacionesAutorizadas,
              this.autorizacionesRechazadas
            ],
            backgroundColor: ['#f59e0b', '#8BC34A', '#ef4444'],
            borderWidth: 0,
            hoverOffset: 8
          }]
        },
        options: {
          responsive: true,
          cutout: '72%',
          plugins: {
            legend: { display: false }
          }
        }
      }
    );
  }

  ir(modulo: string): void {
    if (!this.permisosService.puedeVer(modulo)) {
      return;
    }

    this.navegar.emit(modulo);
  }

  cerrarSesion(): void {
    this.logout.emit();
  }

  irAAutorizaciones(): void {
    this.ir('autorizaciones');
  }

  verInspecciones(): void {
    this.ir('ver-inspecciones');
  }

  irAConductores(): void {
    this.ir('conductores');
  }
}