import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './componentes/login/login';
import { DashboardComponent } from './componentes/dashboard/dashboard';
import { ConductorForm } from './componentes/conductor-form/conductor-form';
import { VehiculosComponent } from './componentes/vehiculos/vehiculos';
import { AutorizacionesComponent } from './componentes/autorizaciones/autorizaciones';
import { ChecklistEditorComponent } from './componentes/checklist-editor/checklist-editor';
import { InspeccionesComponent } from './componentes/inspecciones/inspecciones';
import { ConductoresComponent } from './componentes/conductores/conductores.component';
import { UsuariosComponent } from './componentes/usuarios/usuarios';
import { InspeccionesHistorialComponent as VerInspeccionesComponent }
  from './componentes/inspecciones-historial/inspecciones-historial';
import { ReporteRutaComponent } from './componentes/reportes-ruta/reportes-ruta';
import { IncidentesComponent } from './componentes/incidentes/incidentes';
import { ContactosNotificacionComponent }
  from './componentes/contactos-notificacion/contactos-notificacion';
import { ConfiguracionEmpresaComponent }
  from './componentes/configuracion-empresa/configuracion-empresa';
import { MantenimientoComponent } from './componentes/mantenimiento/mantenimiento';
import { DocumentosComponent } from './componentes/documentos/documentos';
import { AuditoriaComponent } from './componentes/auditoria/auditoria';
import { EncuestaFatigaComponent } from './componentes/encuesta-fatiga/encuesta-fatiga';
import { TrazabilidadComponent } from './componentes/trazabilidad/trazabilidad';
import { CambioRutaComponent } from './componentes/cambio-ruta/cambio-ruta';
import { SolicitudTallerComponent } from './componentes/solicitud-taller/solicitud-taller';
import { PedidosComponent } from './componentes/pedidos/pedidos';
import { SeguimientosRrhhComponent } from './componentes/seguimientos-rrhh/seguimientos-rrhh.component';
import { CyrelesComponent } from './componentes/Cyreles/cyreles.component';
import { CentroInformacionComponent } from './componentes/centro-informacion/centro-informacion';
import { FormatoFGC008Component } from './componentes/formato-fgc008/formato-fgc008';
import { ConfiguracionService } from './services/configuracion.service';
import { AuthService } from './services/auth.service';
import { PermisosService } from './services/permisos.service';
import { environment } from '../environments/environment';

type ModuloPrincipal = 'flota' | 'rrhh' | 'calidad';

interface ModuloItem {
  key: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule, LoginComponent, DashboardComponent, ConductorForm,
    VehiculosComponent, VerInspeccionesComponent, AutorizacionesComponent,
    ReporteRutaComponent, ChecklistEditorComponent, InspeccionesComponent,
    ConductoresComponent, UsuariosComponent, IncidentesComponent,
    ContactosNotificacionComponent, ConfiguracionEmpresaComponent,
    MantenimientoComponent, DocumentosComponent, AuditoriaComponent,
    EncuestaFatigaComponent, TrazabilidadComponent, CambioRutaComponent,
    SolicitudTallerComponent, PedidosComponent,
    SeguimientosRrhhComponent, CyrelesComponent,
    CentroInformacionComponent,
    FormatoFGC008Component
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class AppComponent implements OnInit {

  title = 'Sistema de Gestión de Flota';
  isLoggedIn = false;
  usuarioActivo = '';
  rol = '';
  permisosGranulares: any[] = [];
  moduloActual = 'dashboard';
  sidebarVisible = false;
  empresaNombre = 'Flota';
  empresaLogo: string | null = null;
  colorPrimario = '#15803d';
  moduloPrincipal: ModuloPrincipal = 'flota';

  private readonly baseUrl = environment.fotosUrl;

  readonly tabsPrincipales: { key: ModuloPrincipal; label: string; icon: string }[] = [
    { key: 'flota', label: 'Flota', icon: 'fa-solid fa-truck' },
    { key: 'rrhh', label: 'SST', icon: 'fa-solid fa-shield-halved' },
    { key: 'calidad', label: 'Calidad', icon: 'fa-solid fa-circle-check' },
  ];

  readonly modulosFlota: ModuloItem[] = [
    { key: 'dashboard', label: 'Panel de control', icon: 'fa-solid fa-gauge-high' },
    { key: 'conductores', label: 'Conductores', icon: 'fa-solid fa-id-card' },
    { key: 'vehiculos', label: 'Vehículos', icon: 'fa-solid fa-truck' },
    { key: 'inspecciones', label: 'Inspecciones', icon: 'fa-solid fa-clipboard-check' },
    { key: 'ver-inspecciones', label: 'Historial', icon: 'fa-solid fa-clock-rotate-left' },
    { key: 'autorizaciones', label: 'Autorizaciones', icon: 'fa-solid fa-file-circle-check' },
    { key: 'reporte-ruta', label: 'Reporte en Ruta', icon: 'fa-solid fa-triangle-exclamation' },
    { key: 'cambio-ruta', label: 'Cambio de Ruta', icon: 'fa-solid fa-route' },
    { key: 'incidentes', label: 'Incidentes', icon: 'fa-solid fa-car-burst' },
    { key: 'mantenimiento', label: 'Mantenimiento', icon: 'fa-solid fa-wrench' },
    { key: 'solicitud-taller', label: 'Solicitud Taller', icon: 'fa-solid fa-screwdriver-wrench' },
    { key: 'documentos', label: 'Documentos', icon: 'fa-solid fa-folder-open' },
    { key: 'encuesta-fatiga', label: 'Encuesta Fatiga', icon: 'fa-solid fa-face-tired' },
    { key: 'trazabilidad', label: 'Trazabilidad', icon: 'fa-solid fa-boxes-stacked' },
    { key: 'pedidos', label: 'Pedidos', icon: 'fa-solid fa-box' },
    { key: 'contactos-notificacion', label: 'Contactos WhatsApp', icon: 'fa-brands fa-whatsapp' },
    { key: 'auditoria', label: 'Auditoría', icon: 'fa-solid fa-shield-halved' },
    { key: 'usuarios', label: 'Usuarios', icon: 'fa-solid fa-users-gear' },
    { key: 'configuracion', label: 'Configuración', icon: 'fa-solid fa-sliders' },
    { key: 'checklist', label: 'Checklist', icon: 'fa-solid fa-list-check' },
    { key: 'centro-informacion', label: 'Centro de Información', icon: 'fa-solid fa-book-open' },
  ];

  readonly modulosRrhh: ModuloItem[] = [
    { key: 'rrhh-seguimientos', label: 'Seguimientos SST', icon: 'fa-solid fa-clipboard-list' },
  ];

  readonly modulosCalidad: ModuloItem[] = [
    { key: 'calidad-cyreles', label: 'Cyreles', icon: 'fa-solid fa-box-open' },
    { key: 'calidad-formatos', label: 'Formatos', icon: 'fa-solid fa-file-lines' },
  ];

  private readonly accesoModuloPrincipal: Record<ModuloPrincipal, string[]> = {
    flota: ['Admin', 'Auxiliar', 'Jefe', 'Facturacion', 'Bodega', 'RecursosHumanos', 'PESV', 'Conductor', 'Vendedor', 'Porteria'],
    rrhh: ['Admin', 'RecursosHumanos', 'Jefe', 'PESV', 'SST'],
    calidad: ['Admin', 'Calidad', 'Impresion', 'Jefe', 'PESV'],
  };

  constructor(
    private configuracionService: ConfiguracionService,
    private authService: AuthService,
    private permisosService: PermisosService
  ) { }

  toggleSidebar() { this.sidebarVisible = !this.sidebarVisible; }
  cerrarSidebar() { this.sidebarVisible = false; }

  cambiarModuloPrincipal(mp: ModuloPrincipal) {
    if (!this.puedeVerModuloPrincipal(mp)) return;
    this.moduloPrincipal = mp;
    const primero = this.getModulosPorPrincipal(mp)[0];
    if (primero) this.moduloActual = primero.key;
    this.cerrarSidebar();
  }

  puedeVerModuloPrincipal(mp: ModuloPrincipal): boolean {
    const rolesPermitidos = this.accesoModuloPrincipal[mp];
    if (rolesPermitidos.length === 0) return true;
    return rolesPermitidos.includes(this.rol);
  }

  get tabsVisibles() {
    return this.tabsPrincipales.filter(t => this.puedeVerModuloPrincipal(t.key));
  }

  getModulosPorPrincipal(mp: ModuloPrincipal): ModuloItem[] {
    switch (mp) {
      case 'rrhh': return this.modulosRrhh;
      case 'calidad': return this.modulosCalidad;
      default: return this.modulosFlota;
    }
  }

  get modulosVisibles(): ModuloItem[] {
    return this.getModulosPorPrincipal(this.moduloPrincipal)
      .filter(m => this.tieneAcceso(m.key));
  }

  private get todosModulos(): ModuloItem[] {
    return [...this.modulosFlota, ...this.modulosRrhh, ...this.modulosCalidad];
  }

  private guardarSesion(datos: any) {
    sessionStorage.setItem('user', JSON.stringify(datos));
    sessionStorage.setItem('token', datos.token ?? '');
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    localStorage.removeItem('rol');
  }

  private leerSesion(): any | null {
    const session = sessionStorage.getItem('user');
    if (session) return JSON.parse(session);
    const local = localStorage.getItem('user');
    if (local) { const datos = JSON.parse(local); this.guardarSesion(datos); return datos; }
    return null;
  }

  private limpiarSesion() {
    sessionStorage.removeItem('user'); sessionStorage.removeItem('token');
    localStorage.removeItem('user'); localStorage.removeItem('token');
    localStorage.removeItem('rol');
  }

  cargarConfiguracion() {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        if (data.nombreEmpresa?.trim()) this.empresaNombre = data.nombreEmpresa;
        if (data.logo?.trim()) this.empresaLogo = `${this.baseUrl}/config/${data.logo}`;
        if (data.colorCorporativo) {
          this.colorPrimario = data.colorCorporativo;
          document.documentElement.style.setProperty('--color-primario', data.colorCorporativo);
        }
      },
      error: () => { }
    });
  }

  onLoginSuccess(datos: any) {
    this.isLoggedIn = true;
    this.usuarioActivo = datos.username;
    this.rol = datos.rol;  // ← primero asignar el rol
    this.permisosGranulares = Array.isArray(datos.permisos)
      ? datos.permisos.filter((p: any) => typeof p === 'object' && p.modulo)
      : [];

    // Módulo por defecto
    this.moduloPrincipal = 'flota';
    this.moduloActual = 'dashboard';

    // Roles que no ven dashboard — arrancan en su primer módulo
    if (this.rol === 'Conductor') { this.moduloActual = 'inspecciones'; }
    if (this.rol === 'Vendedor') { this.moduloActual = 'pedidos'; }
    if (this.rol === 'Porteria') { this.moduloActual = 'autorizaciones'; }
    if (this.rol === 'SST') {
      this.moduloPrincipal = 'rrhh'; this.moduloActual = 'rrhh-seguimientos';
    }

    // Calidad e Impresion arrancan en Calidad → Cyreles
    if (this.rol === 'Impresion' || this.rol === 'Calidad') {
      this.moduloPrincipal = 'calidad';
      this.moduloActual = 'calidad-cyreles';
    }

    this.permisosService.cargar(datos);
    this.guardarSesion(datos);
    this.cargarConfiguracion();
  }

  tieneAcceso(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    if (this.permisosGranulares.length > 0) {
      const p = this.permisosGranulares.find((p: any) => p.modulo === modulo);
      return !!p && (p.puedeVer !== false);
    }
    switch (this.rol) {
      case 'Auxiliar':
        return ['dashboard', 'conductores', 'vehiculos', 'inspecciones', 'ver-inspecciones'].includes(modulo);
      case 'Conductor':
        return ['inspecciones', 'reporte-ruta', 'encuesta-fatiga', 'cambio-ruta', 'solicitud-taller', 'pedidos'].includes(modulo);
      case 'Jefe':
        return ['dashboard', 'ver-inspecciones', 'incidentes', 'mantenimiento', 'documentos', 'encuesta-fatiga', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'pedidos', 'calidad-cyreles'].includes(modulo);
      case 'RecursosHumanos':
        return ['dashboard', 'conductores', 'usuarios', 'autorizaciones', 'incidentes', 'documentos', 'encuesta-fatiga', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'rrhh-seguimientos'].includes(modulo);
      case 'Facturacion':
        return ['dashboard', 'autorizaciones', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'pedidos'].includes(modulo);
      case 'Bodega':
        return ['dashboard', 'autorizaciones', 'trazabilidad', 'cambio-ruta', 'solicitud-taller', 'pedidos'].includes(modulo);
      case 'Porteria':
        return ['autorizaciones', 'encuesta-fatiga'].includes(modulo);
      case 'Vendedor':
        return ['pedidos', 'inspecciones', 'reporte-ruta', 'encuesta-fatiga', 'cambio-ruta', 'solicitud-taller', 'autorizaciones'].includes(modulo);
      case 'Calidad':
        return ['calidad-cyreles'].includes(modulo);
      case 'Impresion':
        return ['calidad-cyreles'].includes(modulo);
      case 'SST':
        return ['rrhh-seguimientos'].includes(modulo);
      default:
        return false;
    }
  }

  cambiarModulo(nombreModulo: string) {
    if (!this.tieneAcceso(nombreModulo)) { alert('No tienes permisos para entrar aquí'); return; }
    this.moduloActual = nombreModulo;
    this.cerrarSidebar();
  }

  cerrarSesion() {
    const user = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (user) { const u = JSON.parse(user); this.authService.logout(u.username, u.rol).subscribe(); }
    this.limpiarSesion();
    this.isLoggedIn = false; this.usuarioActivo = ''; this.rol = '';
    this.permisosGranulares = []; this.moduloActual = 'dashboard';
    this.moduloPrincipal = 'flota';
    this.sidebarVisible = false; this.empresaNombre = 'Flota';
    this.empresaLogo = null; this.colorPrimario = '#15803d';
    document.documentElement.style.removeProperty('--color-primario');
  }

  ngOnInit() {
    const datos = this.leerSesion();
    if (datos) this.onLoginSuccess(datos);
    this.cargarConfiguracion();
  }

  getIconoModulo(key: string): string {
    return this.todosModulos.find(m => m.key === key)?.icon ?? 'fa-solid fa-circle';
  }

  getLabelModulo(key: string): string {
    return this.todosModulos.find(m => m.key === key)?.label ?? key;
  }
}