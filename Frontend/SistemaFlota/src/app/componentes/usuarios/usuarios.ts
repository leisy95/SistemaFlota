import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuariosService } from '../../services/usuarios.service';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuarios.html',
  styleUrls: ['./usuarios.scss']
})

export class UsuariosComponent implements OnInit {

  usuarios: any[] = [];
  mostrarModal     = false;
  mostrarRecuperar = false;
  mostrarCambiar   = false;
  editando         = false;
  usuarioEditarId: number | null = null;

  nuevoUsuario = {
    username: '',
    password: '',
    rol:      'Auxiliar',
    email:    '',
    activo:   true,
    permisos: [] as PermisoGranular[]
  };

  emailRecuperar   = '';
  tokenRecuperar   = '';
  nuevaPassword    = '';
  tokenGenerado    = '';
  mensajeRecuperar = '';

  readonly modulos = [
    { key: 'dashboard',              label: '📊 Dashboard' },
    { key: 'inspecciones',           label: '📝 Inspecciones' },
    { key: 'ver-inspecciones',       label: '📋 Historial Inspecciones' },
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

  readonly roles = [
    'Admin', 'Auxiliar', 'Conductor', 'Jefe',
    'Facturacion', 'Bodega', 'Porteria', 'RecursosHumanos'
  ];

  constructor(private usuariosService: UsuariosService) {}

  ngOnInit(): void {
    this.cargarUsuarios();
  }

  cargarUsuarios() {
    this.usuariosService.obtenerUsuarios().subscribe({
      next: (data) => this.usuarios = data,
      error: (err) => console.error(err)
    });
  }

  get totalActivos(): number {
    return this.usuarios.filter(u => u.activo).length;
  }

  // =========================
  // PERMISOS GRANULARES
  // =========================

  getPermiso(modulo: string): PermisoGranular {
    const p = this.nuevoUsuario.permisos.find(p => p.modulo === modulo);
    return p ?? {
      modulo,
      puedeVer:      false,
      puedeCrear:    false,
      puedeEditar:   false,
      puedeEliminar: false
    };
  }

  tienePermiso(modulo: string): boolean {
    return this.nuevoUsuario.permisos.some(p => p.modulo === modulo);
  }

  toggleModulo(modulo: string) {
    const idx = this.nuevoUsuario.permisos.findIndex(p => p.modulo === modulo);
    if (idx >= 0) {
      this.nuevoUsuario.permisos.splice(idx, 1);
    } else {
      this.nuevoUsuario.permisos.push({
        modulo,
        puedeVer:      true,
        puedeCrear:    false,
        puedeEditar:   false,
        puedeEliminar: false
      });
    }
  }

  toggleAccion(modulo: string, accion: 'puedeVer' | 'puedeCrear' | 'puedeEditar' | 'puedeEliminar') {
    const p = this.nuevoUsuario.permisos.find(p => p.modulo === modulo);
    if (!p) return;

    p[accion] = !p[accion];

    // Si desactiva Ver desactiva todo
    if (accion === 'puedeVer' && !p.puedeVer) {
      p.puedeCrear    = false;
      p.puedeEditar   = false;
      p.puedeEliminar = false;
    }

    // Si activa crear/editar/eliminar activa Ver automáticamente
    if ((accion === 'puedeCrear' || accion === 'puedeEditar' || accion === 'puedeEliminar') && p[accion]) {
      p.puedeVer = true;
    }
  }

  seleccionarTodos() {
    this.nuevoUsuario.permisos = this.modulos.map(m => ({
      modulo:        m.key,
      puedeVer:      true,
      puedeCrear:    true,
      puedeEditar:   true,
      puedeEliminar: true
    }));
  }

  soloLectura() {
    this.nuevoUsuario.permisos = this.modulos.map(m => ({
      modulo:        m.key,
      puedeVer:      true,
      puedeCrear:    false,
      puedeEditar:   false,
      puedeEliminar: false
    }));
  }

  deseleccionarTodos() {
    this.nuevoUsuario.permisos = [];
  }

  // =========================
  // NUEVO USUARIO
  // =========================

  agregarUsuario() {
    this.editando        = false;
    this.usuarioEditarId = null;
    this.nuevoUsuario = {
      username: '', password: '', rol: 'Auxiliar',
      email: '', activo: true, permisos: []
    };
    this.mostrarModal = true;
  }

  // =========================
  // GUARDAR
  // =========================

  guardarUsuario() {
    if (!this.nuevoUsuario.username)                   { alert('Ingrese el nombre de usuario');  return; }
    if (!this.editando && !this.nuevoUsuario.password) { alert('Ingrese la contraseña');          return; }
    if (!this.nuevoUsuario.email)                      { alert('Ingrese el correo electrónico'); return; }

    if (this.editando) {
      this.usuariosService
        .actualizarUsuario(this.usuarioEditarId!, this.nuevoUsuario)
        .subscribe({
          next: () => { this.cargarUsuarios(); this.cerrarModal(); },
          error: (err) => console.error(err)
        });
    } else {
      this.usuariosService
        .crearUsuario(this.nuevoUsuario)
        .subscribe({
          next: () => { this.cargarUsuarios(); this.cerrarModal(); },
          error: (err) => { console.error(err); alert(err.error || 'Error creando usuario'); }
        });
    }
  }

  // =========================
  // EDITAR
  // =========================

  editarUsuario(usuario: any) {
    this.editando        = true;
    this.usuarioEditarId = usuario.id;

    const permisos: PermisoGranular[] = (usuario.permisos ?? []).map((p: any) => ({
      modulo:        p.modulo,
      puedeVer:      p.puedeVer      ?? true,
      puedeCrear:    p.puedeCrear    ?? false,
      puedeEditar:   p.puedeEditar   ?? false,
      puedeEliminar: p.puedeEliminar ?? false
    }));

    this.nuevoUsuario = {
      username: usuario.username,
      password: '',
      rol:      usuario.rol,
      email:    usuario.email ?? '',
      activo:   usuario.activo,
      permisos
    };
    this.mostrarModal = true;
  }

  // =========================
  // ELIMINAR
  // =========================

  eliminarUsuario(id: number) {
    if (!confirm('¿Eliminar usuario?')) return;
    this.usuariosService.eliminarUsuario(id).subscribe({
      next: () => this.cargarUsuarios(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // CAMBIAR ESTADO
  // =========================

  cambiarEstado(usuario: any) {
    this.usuariosService.cambiarEstado(usuario.id).subscribe({
      next: () => this.cargarUsuarios(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // RECUPERAR CONTRASEÑA
  // =========================

  abrirRecuperar() {
    this.emailRecuperar   = '';
    this.tokenGenerado    = '';
    this.mensajeRecuperar = '';
    this.mostrarRecuperar = true;
    this.mostrarCambiar   = false;
  }

  solicitarRecuperacion() {
    if (!this.emailRecuperar) { alert('Ingrese el correo'); return; }

    this.usuariosService.solicitarRecuperacion(this.emailRecuperar).subscribe({
      next: (data) => {
        this.tokenGenerado    = data.token;
        this.mensajeRecuperar = `Token: ${data.token}`;
        this.mostrarCambiar   = true;
      },
      error: (err) => alert(err.error || 'Error solicitando recuperación')
    });
  }

  cambiarPassword() {
    if (!this.tokenRecuperar) { alert('Ingrese el token');            return; }
    if (!this.nuevaPassword)  { alert('Ingrese la nueva contraseña'); return; }

    this.usuariosService.cambiarPassword(
      this.emailRecuperar, this.tokenRecuperar, this.nuevaPassword
    ).subscribe({
      next: () => {
        alert('Contraseña cambiada correctamente');
        this.mostrarRecuperar = false;
        this.mostrarCambiar   = false;
      },
      error: (err) => alert(err.error || 'Token inválido o expirado')
    });
  }

  cerrarModal() {
    this.mostrarModal     = false;
    this.mostrarRecuperar = false;
    this.mostrarCambiar   = false;
  }

  getBadgeRol(rol: string): string {
    switch (rol) {
      case 'Admin':           return 'badge-admin';
      case 'Auxiliar':        return 'badge-auxiliar';
      case 'Conductor':       return 'badge-conductor';
      case 'Jefe':            return 'badge-jefe';
      case 'Facturacion':     return 'badge-facturacion';
      case 'Bodega':          return 'badge-bodega';
      case 'Porteria':        return 'badge-porteria';
      case 'RecursosHumanos': return 'badge-rrhh';
      default:                return 'badge-auxiliar';
    }
  }
}

interface PermisoGranular {
  modulo:        string;
  puedeVer:      boolean;
  puedeCrear:    boolean;
  puedeEditar:   boolean;
  puedeEliminar: boolean;
}