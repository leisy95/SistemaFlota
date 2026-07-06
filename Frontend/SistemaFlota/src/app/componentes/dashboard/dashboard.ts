import {
  Component, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef
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
export class DashboardComponent implements OnInit, OnDestroy {

  @ViewChild('graficoInspecciones') canvasInspecciones!: ElementRef;
  @ViewChild('graficoDona') canvasDona!: ElementRef;

  @Output() logout = new EventEmitter<void>();
  @Output() navegar = new EventEmitter<string>();

  horaActual = '';
  fechaActual = '';
  rolUsuario = '';
  private intervalo: any;

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
    { key: 'ver-inspecciones', label: 'Historial', icon: 'fa-solid fa-clock-rotate-left', color: 'cyan' },
  ];

  // Accesos rápidos filtrados por rol
  modulosRapidos: any[] = [];

  private readonly accesosPorRol: Record<string, string[]> = {
    Admin: ['inspecciones', 'autorizaciones', 'reporte-ruta', 'mantenimiento', 'documentos', 'conductores'],
    Jefe: ['autorizaciones', 'incidentes', 'mantenimiento', 'documentos', 'trazabilidad'],
    Conductor: ['inspecciones', 'reporte-ruta', 'encuesta-fatiga', 'cambio-ruta', 'solicitud-taller', 'pedidos'],
    Facturacion: ['autorizaciones', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'pedidos'],
    Bodega: ['autorizaciones', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'pedidos'],
    Porteria: ['autorizaciones', 'encuesta-fatiga'],
    Vendedor: ['pedidos', 'inspecciones', 'reporte-ruta', 'encuesta-fatiga', 'cambio-ruta', 'solicitud-taller', 'autorizaciones'],
    RecursosHumanos: ['conductores', 'autorizaciones', 'incidentes', 'documentos', 'encuesta-fatiga', 'trazabilidad'],
    Auxiliar: ['inspecciones', 'ver-inspecciones', 'conductores'],
    PESV: ['inspecciones', 'autorizaciones', 'reporte-ruta', 'mantenimiento', 'documentos', 'conductores'],
  };

  constructor(
    private conductoresService: ConductoresService,
    private vehiculosService: VehiculosService,
    private inspeccionesService: InspeccionesService,
    private autorizacionesService: AutorizacionesService,
    private incidentesService: IncidentesService,
    private mantenimientoService: MantenimientoService,
    private documentosService: DocumentosService
  ) { }

  ngOnInit() {
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (raw) this.rolUsuario = JSON.parse(raw).rol;
    this.cargarModulosRapidos();
    this.actualizarHora();
    this.intervalo = setInterval(() => this.actualizarHora(), 1000);
    this.cargarDatos();
    setTimeout(() => this.crearGraficas(), 600);
  }

  ngOnDestroy() { if (this.intervalo) clearInterval(this.intervalo); }

  cargarModulosRapidos() {
    const keys = this.rolUsuario === 'Admin' || this.rolUsuario === 'PESV'
      ? this.accesosPorRol['Admin']
      : (this.accesosPorRol[this.rolUsuario] ?? []);
    this.modulosRapidos = keys
      .map(k => this.todosModos.find(m => m.key === k))
      .filter(Boolean) as any[];
  }

  actualizarHora() {
    const ahora = new Date();
    this.horaActual = ahora.toLocaleTimeString('es-CO', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
    this.fechaActual = ahora.toLocaleDateString('es-CO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' });
  }

  cargarDatos() {
    this.conductoresService.obtenerTotalConductores().subscribe({
      next: (data: any) => {
        const arr = Array.isArray(data) ? data : (data.data ?? []);
        this.totalConductores = arr.length;
      },
      error: (err) => console.error(err)
    });

    this.vehiculosService.obtenerTotalVehiculos().subscribe({
      next: (data: any) => {
        const arr = Array.isArray(data) ? data : (data.data ?? []);
        this.totalVehiculos = arr.length;
      },
      error: (err) => console.error(err)
    });

    this.inspeccionesService.obtenerTotalInspecciones().subscribe({
      next: (data: any) => {
        const arr = Array.isArray(data) ? data : (data.data ?? []);
        this.totalInspecciones = arr.length;
        this.ultimasInspecciones = arr.slice(0, 5);
      },
      error: (err) => console.error(err)
    });

    this.autorizacionesService.obtenerAutorizaciones().subscribe({
      next: (res: any) => {
        const data: any[] = Array.isArray(res) ? res : (res.data ?? []);
        const hoy = new Date().toDateString();
        this.autorizacionesHoy = data.filter(a => new Date(a.fechaCreacion).toDateString() === hoy).length;
        this.autorizacionesPendientes = data.filter(a => a.estado === 'Pendiente').length;
        this.autorizacionesAutorizadas = data.filter(a => a.estado === 'Autorizado').length;
        this.autorizacionesRechazadas = data.filter(a => a.estado === 'Rechazado').length;
        this.ultimasAutorizaciones = data.slice(0, 5);
      },
      error: (err) => console.error(err)
    });

    this.incidentesService.obtenerIncidentes().subscribe({
      next: (data: any) => {
        const arr = Array.isArray(data) ? data : (data.data ?? []);
        this.incidentesPendientes = arr.filter((i: any) => i.estado === 'Pendiente').length;
      },
      error: (err) => console.error(err)
    });

    this.mantenimientoService.obtenerTodos().subscribe({
      next: (data: any) => {
        const arr = Array.isArray(data) ? data : (data.data ?? []);
        this.vehiculosEnTaller = arr.filter((m: any) => m.estado === 'EnTaller').length;
      },
      error: (err) => console.error(err)
    });

    this.documentosService.obtenerPorVencer().subscribe({
      next: (data: any) => this.documentosPorVencer = data.total ?? 0,
      error: (err) => console.error(err)
    });
  }

  crearGraficas() { this.crearGraficoBarras(); this.crearGraficoDona(); }

  crearGraficoBarras() {
    if (!this.canvasInspecciones?.nativeElement) return;
    new Chart(this.canvasInspecciones.nativeElement, {
      type: 'bar',
      data: {
        labels: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun'],
        datasets: [{
          label: 'Inspecciones',
          data: [5, 8, 6, 10, 7, 12],
          backgroundColor: '#90A4AE',
          borderColor: '#558B2F',
          borderWidth: 1, borderRadius: 8, borderSkipped: false,
        }]
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: 'rgba(255,255,255,0.05)' }, ticks: { color: '#64748b' } },
          x: { grid: { display: false }, ticks: { color: '#64748b' } }
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
          data: [this.autorizacionesPendientes || 1, this.autorizacionesAutorizadas || 1, this.autorizacionesRechazadas || 0],
          backgroundColor: ['#f59e0b', '#8BC34A', '#ef4444'],
          borderWidth: 0, hoverOffset: 8
        }]
      },
      options: { responsive: true, cutout: '72%', plugins: { legend: { display: false } } }
    });
  }

  ir(modulo: string) { this.navegar.emit(modulo); }
  cerrarSesion() { this.logout.emit(); }
  irAAutorizaciones() { this.navegar.emit('autorizaciones'); }
  verInspecciones() { this.navegar.emit('ver-inspecciones'); }
  irAConductores() { this.navegar.emit('conductores'); }
}