import {
  Component,
  OnInit
} from '@angular/core';

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
import { ReporteRutaComponent }
  from './componentes/reportes-ruta/reportes-ruta';
import { IncidentesComponent }
  from './componentes/incidentes/incidentes';
import { ContactosNotificacionComponent }
  from './componentes/contactos-notificacion/contactos-notificacion';
import { ConfiguracionEmpresaComponent }
  from './componentes/configuracion-empresa/configuracion-empresa';
import { MantenimientoComponent }
  from './componentes/mantenimiento/mantenimiento';
import { DocumentosComponent }
  from './componentes/documentos/documentos';
import { AuditoriaComponent }
  from './componentes/auditoria/auditoria';
import { EncuestaFatigaComponent }
  from './componentes/encuesta-fatiga/encuesta-fatiga';
import { TrazabilidadComponent }
  from './componentes/trazabilidad/trazabilidad';
import { CambioRutaComponent }
  from './componentes/cambio-ruta/cambio-ruta';
import { SolicitudTallerComponent }
  from './componentes/solicitud-taller/solicitud-taller';
import { ConfiguracionService }
  from './services/configuracion.service';
import { AuthService }
  from './services/auth.service';
import { PermisosService } from './services/permisos.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    LoginComponent,
    DashboardComponent,
    ConductorForm,
    VehiculosComponent,
    VerInspeccionesComponent,
    AutorizacionesComponent,
    ReporteRutaComponent,
    ChecklistEditorComponent,
    InspeccionesComponent,
    ConductoresComponent,
    UsuariosComponent,
    IncidentesComponent,
    ContactosNotificacionComponent,
    ConfiguracionEmpresaComponent,
    MantenimientoComponent,
    DocumentosComponent,
    AuditoriaComponent,
    EncuestaFatigaComponent,
    TrazabilidadComponent,
    CambioRutaComponent,
    SolicitudTallerComponent
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})

export class AppComponent implements OnInit {

  title = 'Sistema de Gestión de Flota';

  isLoggedIn     = false;
  usuarioActivo  = '';
  rol            = '';
  permisos:      string[] = [];
  moduloActual   = 'dashboard';
  sidebarVisible = false;

  empresaNombre  = 'Flota';
  empresaLogo:   string | null = null;
  colorPrimario  = '#0f172a';

  readonly todosModulos = [
    { key: 'dashboard',              label: '📊 Dashboard' },
    { key: 'inspecciones',           label: '📝 Inspecciones' },
    { key: 'ver-inspecciones',       label: '📋 Historial' },
    { key: 'autorizaciones',         label: '✅ Autorizaciones' },
    { key: 'reporte-ruta',           label: '🚨 Reporte en Ruta' },
    { key: 'cambio-ruta',            label: '🔄 Cambio de Ruta' },
    { key: 'incidentes',             label: '⚠️ Incidentes' },
    { key: 'contactos-notificacion', label: '📱 Contactos WhatsApp' },
    { key: 'mantenimiento',          label: '🔧 Taller' },
    { key: 'solicitud-taller',       label: '🛠️ Solicitud Taller' },
    { key: 'documentos',             label: '📁 Documentos' },
    { key: 'encuesta-fatiga',        label: '😴 Encuesta Fatiga' },
    { key: 'trazabilidad',           label: '📦 Trazabilidad' },
    { key: 'auditoria',              label: '🔍 Auditoría' },
    { key: 'configuracion',          label: '⚙️ Configuración' },
    { key: 'conductores',            label: '👤 Conductores' },
    { key: 'vehiculos',              label: '🚚 Vehículos' },
    { key: 'usuarios',               label: '👥 Usuarios' },
    { key: 'checklist',              label: '✅ Checklist' },
  ];

  constructor(
  private configuracionService: ConfiguracionService,
  private authService:          AuthService,
  private permisosService:      PermisosService
) {}

  toggleSidebar() { this.sidebarVisible = !this.sidebarVisible; }
  cerrarSidebar()  { this.sidebarVisible = false; }

  cargarConfiguracion() {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        if (data.nombreEmpresa && data.nombreEmpresa.trim() !== '')
          this.empresaNombre = data.nombreEmpresa;
        if (data.logo && data.logo.trim() !== '')
          this.empresaLogo = `https://localhost:7293/config/${data.logo}`;
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
  this.permisos      = datos.permisos ?? [];
  this.moduloActual  = 'dashboard';
  this.permisosService.cargar(datos); // ← NUEVO
  this.cargarConfiguracion();
}

  tieneAcceso(modulo: string): boolean {
    if (this.rol === 'Admin') return true;
    if (this.permisos.length > 0) return this.permisos.includes(modulo);

    switch (this.rol) {
      case 'Auxiliar': return [
        'dashboard', 'conductores', 'vehiculos',
        'inspecciones', 'ver-inspecciones'
      ].includes(modulo);
      case 'Conductor': return [
        'dashboard', 'inspecciones', 'reporte-ruta',
        'encuesta-fatiga', 'cambio-ruta', 'solicitud-taller'
      ].includes(modulo);
      case 'Jefe': return [
        'dashboard', 'ver-inspecciones', 'incidentes',
        'mantenimiento', 'documentos', 'encuesta-fatiga',
        'trazabilidad', 'cambio-ruta', 'solicitud-taller'
      ].includes(modulo);
      case 'RecursosHumanos': return [
        'dashboard', 'conductores', 'usuarios',
        'autorizaciones', 'incidentes', 'documentos',
        'encuesta-fatiga', 'trazabilidad', 'cambio-ruta',
        'solicitud-taller'
      ].includes(modulo);
      case 'Facturacion': return [
        'dashboard', 'autorizaciones', 'trazabilidad',
        'cambio-ruta', 'solicitud-taller'
      ].includes(modulo);
      case 'Bodega': return [
        'dashboard', 'autorizaciones', 'trazabilidad',
        'cambio-ruta', 'solicitud-taller'
      ].includes(modulo);
      case 'Porteria': return [
        'dashboard', 'autorizaciones', 'encuesta-fatiga'
      ].includes(modulo);
      default: return false;
    }
  }

  get modulosVisibles() {
    return this.todosModulos.filter(m => this.tieneAcceso(m.key));
  }

  cambiarModulo(nombreModulo: string) {
    if (!this.tieneAcceso(nombreModulo)) {
      alert('No tienes permisos para entrar aquí');
      return;
    }
    this.moduloActual = nombreModulo;
    this.cerrarSidebar();
  }

  cerrarSesion() {
    const user = localStorage.getItem('user');
    if (user) {
      const userData = JSON.parse(user);
      this.authService.logout(userData.username, userData.rol).subscribe();
    }

    localStorage.removeItem('user');
    localStorage.removeItem('token');
    this.isLoggedIn     = false;
    this.usuarioActivo  = '';
    this.rol            = '';
    this.permisos       = [];
    this.moduloActual   = 'dashboard';
    this.sidebarVisible = false;
    this.empresaNombre  = 'Flota';
    this.empresaLogo    = null;
    this.colorPrimario  = '#0f172a';
    document.documentElement.style.removeProperty('--color-primario');
  }

  ngOnInit() {
  const data = localStorage.getItem('user');
  if (data) {
    const user = JSON.parse(data);
    this.onLoginSuccess(user);
    this.permisosService.cargar(user); // ← NUEVO
  }
  this.cargarConfiguracion();
}

}