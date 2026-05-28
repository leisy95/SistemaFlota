import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  ViewChild,
  ElementRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { Chart } from 'chart.js/auto';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { InspeccionesService } from '../../services/inspecciones.service';
import { AutorizacionesService } from '../../services/autorizaciones.service';
import { IncidentesService } from '../../services/incidentes.service';
import { MantenimientoService } from '../../services/mantenimiento.service';
import { DocumentosService } from '../../services/documentos.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})

export class DashboardComponent implements OnInit {

  @ViewChild('graficoInspecciones') canvasInspecciones!: ElementRef;
  @ViewChild('graficoDona')         canvasDona!: ElementRef;

  @Output() logout  = new EventEmitter<void>();
  @Output() navegar = new EventEmitter<string>();

  // HORA EN TIEMPO REAL
  horaActual = '';
  fechaActual = '';
  private intervalo: any;

  // KPIs
  totalConductores   = 0;
  totalVehiculos     = 0;
  totalInspecciones  = 0;
  autorizacionesHoy  = 0;

  // ALERTAS
  documentosPorVencer = 0;
  vehiculosEnTaller   = 0;
  incidentesPendientes = 0;

  // ÚLTIMAS ACTIVIDADES
  ultimasInspecciones:  any[] = [];
  ultimasAutorizaciones: any[] = [];

  // STATS PARA DONA
  autorizacionesPendientes  = 0;
  autorizacionesAutorizadas = 0;
  autorizacionesRechazadas  = 0;

  readonly modulosRapidos = [
    { key: 'inspecciones',    label: 'Nueva Inspección',   icon: '📝', color: 'blue'   },
    { key: 'autorizaciones',  label: 'Autorizaciones',     icon: '✅', color: 'green'  },
    { key: 'reporte-ruta',    label: 'Reporte en Ruta',    icon: '🚨', color: 'red'    },
    { key: 'mantenimiento',   label: 'Taller',             icon: '🔧', color: 'yellow' },
    { key: 'documentos',      label: 'Documentos',         icon: '📁', color: 'purple' },
    { key: 'conductores',     label: 'Conductores',        icon: '👤', color: 'blue'   },
  ];

  constructor(
    private conductoresService:   ConductoresService,
    private vehiculosService:     VehiculosService,
    private inspeccionesService:  InspeccionesService,
    private autorizacionesService: AutorizacionesService,
    private incidentesService:    IncidentesService,
    private mantenimientoService: MantenimientoService,
    private documentosService:    DocumentosService
  ) {}

  ngOnInit() {
    this.actualizarHora();
    this.intervalo = setInterval(() => this.actualizarHora(), 1000);
    this.cargarDatos();
    setTimeout(() => {
      this.crearGraficas();
    }, 500);
  }

  ngOnDestroy() {
    if (this.intervalo) clearInterval(this.intervalo);
  }

  actualizarHora() {
    const ahora = new Date();
    this.horaActual  = ahora.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    this.fechaActual = ahora.toLocaleDateString('es-CO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  cargarDatos() {

    this.conductoresService.obtenerTotalConductores().subscribe({
      next: (data) => this.totalConductores = data.length,
      error: (err) => console.error(err)
    });

    this.vehiculosService.obtenerTotalVehiculos().subscribe({
      next: (data) => this.totalVehiculos = data.length,
      error: (err) => console.error(err)
    });

    this.inspeccionesService.obtenerTotalInspecciones().subscribe({
      next: (data) => {
        this.totalInspecciones = data.length;
        this.ultimasInspecciones = data.slice(0, 5);
      },
      error: (err) => console.error(err)
    });

    this.autorizacionesService.obtenerAutorizaciones().subscribe({
      next: (data: any[]) => {
        const hoy = new Date().toDateString();
        this.autorizacionesHoy = data.filter(a =>
          new Date(a.fechaCreacion).toDateString() === hoy
        ).length;
        this.autorizacionesPendientes  = data.filter(a => a.estado === 'Pendiente').length;
        this.autorizacionesAutorizadas = data.filter(a => a.estado === 'Autorizado').length;
        this.autorizacionesRechazadas  = data.filter(a => a.estado === 'Rechazado').length;
        this.ultimasAutorizaciones     = data.slice(0, 5);
      },
      error: (err) => console.error(err)
    });

    this.incidentesService.obtenerIncidentes().subscribe({
      next: (data: any[]) => {
        this.incidentesPendientes = data.filter(i => i.estado === 'Pendiente').length;
      },
      error: (err) => console.error(err)
    });

    this.mantenimientoService.obtenerTodos().subscribe({
      next: (data: any[]) => {
        this.vehiculosEnTaller = data.filter(m => m.estado === 'EnTaller').length;
      },
      error: (err) => console.error(err)
    });

    this.documentosService.obtenerPorVencer().subscribe({
      next: (data: any) => {
        this.documentosPorVencer = data.total ?? 0;
      },
      error: (err) => console.error(err)
    });
  }

  crearGraficas() {
    this.crearGraficoBarras();
    this.crearGraficoDona();
  }

  crearGraficoBarras() {
    if (!this.canvasInspecciones?.nativeElement) return;
    new Chart(this.canvasInspecciones.nativeElement, {
      type: 'bar',
      data: {
        labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
        datasets: [{
          label: 'Inspecciones',
          data: [5, 8, 6, 10, 7, 12],
          backgroundColor: 'rgba(59, 130, 246, 0.8)',
          borderRadius: 8,
          borderSkipped: false,
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: { display: false }
        },
        scales: {
          y: { beginAtZero: true, grid: { color: '#f1f5f9' } },
          x: { grid: { display: false } }
        }
      }
    });
  }

  crearGraficoDona() {
    if (!this.canvasDona?.nativeElement) return;
    new Chart(this.canvasDona.nativeElement, {
      type: 'doughnut',
      data: {
        labels: ['Pendientes', 'Autorizadas', 'Rechazadas'],
        datasets: [{
          data: [
            this.autorizacionesPendientes  || 1,
            this.autorizacionesAutorizadas || 1,
            this.autorizacionesRechazadas  || 0
          ],
          backgroundColor: ['#f59e0b', '#15803d', '#dc2626'],
          borderWidth: 0,
          hoverOffset: 8
        }]
      },
      options: {
        responsive: true,
        cutout: '70%',
        plugins: {
          legend: { position: 'bottom' }
        }
      }
    });
  }

  ir(modulo: string) {
    this.navegar.emit(modulo);
  }

  cerrarSesion() { this.logout.emit(); }
  irAInspecciones()    { this.navegar.emit('inspecciones'); }
  irAAutorizaciones()  { this.navegar.emit('autorizaciones'); }
  irAReportes()        { this.navegar.emit('reporte-ruta'); }
  irAChecklist()       { this.navegar.emit('checklist'); }
  irAAutorizadores()   { this.navegar.emit('usuarios'); }
  irADocumentos()      { this.navegar.emit('documentos'); }
  irAConfiguracion()   { this.navegar.emit('configuracion'); }
  verInspecciones()    { this.navegar.emit('ver-inspecciones'); }
  irAConductores()     { this.navegar.emit('conductores'); }
  irAGestionUsuarios() { this.navegar.emit('usuarios'); }
}