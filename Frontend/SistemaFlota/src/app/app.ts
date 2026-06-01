import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent }           from './componentes/login/login';
import { DashboardComponent }       from './componentes/dashboard/dashboard';
import { ConductorForm }            from './componentes/conductor-form/conductor-form';
import { VehiculosComponent }       from './componentes/vehiculos/vehiculos';
import { AutorizacionesComponent }  from './componentes/autorizaciones/autorizaciones';
import { ChecklistEditorComponent } from './componentes/checklist-editor/checklist-editor';
import { InspeccionesComponent }    from './componentes/inspecciones/inspecciones';
import { ConductoresComponent }     from './componentes/conductores/conductores.component';
import { UsuariosComponent }        from './componentes/usuarios/usuarios';
import { InspeccionesHistorialComponent as VerInspeccionesComponent }
  from './componentes/inspecciones-historial/inspecciones-historial';
import { ReporteRutaComponent }     from './componentes/reportes-ruta/reportes-ruta';
import { IncidentesComponent }      from './componentes/incidentes/incidentes';
import { ContactosNotificacionComponent }
  from './componentes/contactos-notificacion/contactos-notificacion';
import { ConfiguracionEmpresaComponent }
  from './componentes/configuracion-empresa/configuracion-empresa';
import { MantenimientoComponent }   from './componentes/mantenimiento/mantenimiento';
import { DocumentosComponent }      from './componentes/documentos/documentos';
import { AuditoriaComponent }       from './componentes/auditoria/auditoria';
import { EncuestaFatigaComponent }  from './componentes/encuesta-fatiga/encuesta-fatiga';
import { TrazabilidadComponent }    from './componentes/trazabilidad/trazabilidad';
import { CambioRutaComponent }      from './componentes/cambio-ruta/cambio-ruta';
import { SolicitudTallerComponent } from './componentes/solicitud-taller/solicitud-taller';
import { ConfiguracionService }     from './services/configuracion.service';
import { AuthService }              from './services/auth.service';
import { PermisosService }          from './services/permisos.service';
import { environment }              from '../environments/environment';

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
    SolicitudTallerComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class AppComponent implements OnInit {

  title          = 'Sistema de Gestión de Flota';
  isLoggedIn     = false;
  usuarioActivo  = '';
  rol            = '';
  permisosGranulares: any[] = [];
  moduloActual   = 'dashboard';
  sidebarVisible = false;
  empresaNombre  = 'Flota';
  empresaLogo:   string | null = null;
  colorPrimario  = '#15803d';

  // ── URL base dinámica desde environment ──────────────────────────────────
  private readonly baseUrl = environment.apiUrl.replace('/api', '');

  readonly todosModulos = [
    { key: 'dashboard',              label: 'Panel de control',       icon: 'fa-solid fa-gauge-high' },
    { key: 'conductores',            label: 'Conductores',            icon: 'fa-solid fa-id-card' },
    { key: 'vehiculos',              label: 'Vehículos',              icon: 'fa-solid fa-truck' },
    { key: 'inspecciones',           label: 'Inspecciones',           icon: 'fa-solid fa-clipboard-check' },
    { key: 'ver-inspecciones',       label: 'Historial',              icon: 'fa-solid fa-clock-rotate-left' },
    { key: 'autorizaciones',         label: 'Autorizaciones',         icon: 'fa-solid fa-file-circle-check' },
    { key: 'reporte-ruta',           label: 'Reporte en Ruta',        icon: 'fa-solid fa-triangle-exclamation' },
    { key: 'cambio-ruta',            label: 'Cambio de Ruta',         icon: 'fa-solid fa-route' },
    { key: 'incidentes',             label: 'Incidentes',             icon: 'fa-solid fa-car-burst' },
    { key: 'mantenimiento',          label: 'Mantenimiento',          icon: 'fa-solid fa-wrench' },
    { key: 'solicitud-taller',       label: 'Solicitud Taller',       icon: 'fa-solid fa-screwdriver-wrench' },
    { key: 'documentos',             label: 'Documentos',             icon: 'fa-solid fa-folder-open' },
    { key: 'encuesta-fatiga',        label: 'Encuesta Fatiga',        icon: 'fa-solid fa-face-tired' },
    { key: 'trazabilidad',           label: 'Trazabilidad',           icon: 'fa-solid fa-boxes-stacked' },
    { key: 'contactos-notificacion', label: 'Contactos WhatsApp',     icon: 'fa-brands fa-whatsapp' },
    { key: 'auditoria',              label: 'Auditoría',              icon: 'fa-solid fa-shield-halved' },
    { key: 'usuarios',               label: 'Usuarios',               icon: 'fa-solid fa-users-gear' },
    { key: 'configuracion',          label: 'Configuración',          icon: 'fa-solid fa-sliders' },
    { key: 'checklist',              label: 'Checklist',              icon: 'fa-solid fa-list-check' },
  ];

  constructor(
    private configuracionService: ConfiguracionService,
    private authService:          AuthService,
    private permisosService:      PermisosService
  ) {}

  toggleSidebar() { this.sidebarVisible = !this.sidebarVisible; }
  cerrarSidebar()  { this.sidebarVisible = false; }

  private guardarSesion(datos: any) {
    sessionStorage.setItem('user',  JSON.stringify(datos));
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
    localStorage.removeItem('user');  localStorage.removeItem('token'); localStorage.removeItem('rol');
  }

  cargarConfiguracion() {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        if (data.nombreEmpresa?.trim()) this.empresaNombre = data.nombreEmpresa;
        // ── URL dinámica — usa baseUrl del environment ─────────────────────
        if (data.logo?.trim()) this.empresaLogo = `${this.baseUrl}/config/${data.logo}`;
        if (data.colorCorporativo) {
          this.colorPrimario = data.colorCorporativo;
          document.documentElement.style.setProperty('--color-primario', data.colorCorporativo);
        }
      },
      error: () => {}
    });
  }

  onLoginSuccess(datos: any) {
    this.isLoggedIn    = true;
    this.usuarioActivo = datos.username;
    this.rol           = datos.rol;
    this.permisosGranulares = Array.isArray(datos.permisos)
      ? datos.permisos.filter((p: any) => typeof p === 'object' && p.modulo)
      : [];
    this.moduloActual = 'dashboard';
    this.permisosService.cargar(datos);
    this.guardarSesion(datos);
    this.cargarConfiguracion();
  }

  tieneAcceso(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    if (this.rol === 'PESV')  return true;
    if (this.permisosGranulares.length > 0) {
      const p = this.permisosGranulares.find((p: any) => p.modulo === modulo);
      return !!p && (p.puedeVer !== false);
    }
    switch (this.rol) {
      case 'Auxiliar':        return ['dashboard','conductores','vehiculos','inspecciones','ver-inspecciones'].includes(modulo);
      case 'Conductor':       return ['dashboard','inspecciones','reporte-ruta','encuesta-fatiga','cambio-ruta','solicitud-taller'].includes(modulo);
      case 'Jefe':            return ['dashboard','ver-inspecciones','incidentes','mantenimiento','documentos','encuesta-fatiga','trazabilidad','cambio-ruta','solicitud-taller'].includes(modulo);
      case 'RecursosHumanos': return ['dashboard','conductores','usuarios','autorizaciones','incidentes','documentos','encuesta-fatiga','trazabilidad','cambio-ruta','solicitud-taller'].includes(modulo);
      case 'Facturacion':     return ['dashboard','autorizaciones','trazabilidad','cambio-ruta','solicitud-taller'].includes(modulo);
      case 'Bodega':          return ['dashboard','autorizaciones','trazabilidad','cambio-ruta','solicitud-taller'].includes(modulo);
      case 'Porteria':        return ['dashboard','autorizaciones','encuesta-fatiga'].includes(modulo);
      default: return false;
    }
  }

  get modulosVisibles() { return this.todosModulos.filter(m => this.tieneAcceso(m.key)); }

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