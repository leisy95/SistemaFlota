import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UsuariosService } from '../../../core/services/usuarios.service';

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './usuarios.html',
  styleUrls: ['./usuarios.scss']
})
export class UsuariosComponent implements OnInit {

  usuarios: any[] = [];
  mostrarModal = false;
  mostrarRecuperar = false;
  mostrarCambiar = false;
  editando = false;
  usuarioEditarId: number | null = null;

  paginaActual = 1;
  porPagina = 10;
  totalUsuarios = 0;
  totalRegistros = 0;
  totalPaginas = 0;
  buscar = '';

  nuevoUsuario = {
    username: '',
    password: '',
    rol: 'Auxiliar',
    email: '',
    activo: true,
    permisos: [] as PermisoGranular[]
  };

  emailRecuperar = '';
  tokenRecuperar = '';
  nuevaPassword = '';
  passwordConductores = '';
  tokenGenerado = '';
  mensajeRecuperar = '';

  readonly modulos = [
    // FLOTA / GENERAL
    { key: 'dashboard', label: 'Dashboard', grupo: 'GENERAL' },
    { key: 'conductores', label: 'Conductores', grupo: 'GENERAL' },
    { key: 'vehiculos', label: 'Vehículos', grupo: 'GENERAL' },
    { key: 'inspecciones', label: 'Inspecciones', grupo: 'GENERAL' },
    { key: 'ver-inspecciones', label: 'Historial Inspecciones', grupo: 'GENERAL' },
    { key: 'autorizaciones', label: 'Autorizaciones', grupo: 'GENERAL' },
    { key: 'reporte-ruta', label: 'Reporte en Ruta', grupo: 'GENERAL' },
    { key: 'incidentes', label: 'Incidentes', grupo: 'GENERAL' },
    { key: 'cambio-ruta', label: 'Cambio de Ruta', grupo: 'GENERAL' },
    { key: 'solicitud-taller', label: 'Solicitud Taller', grupo: 'GENERAL' },
    { key: 'mantenimiento', label: 'Taller', grupo: 'GENERAL' },
    { key: 'documentos', label: 'Documentos', grupo: 'GENERAL' },
    { key: 'encuesta-fatiga', label: 'Encuesta Fatiga', grupo: 'GENERAL' },
    { key: 'checklist', label: 'Checklist', grupo: 'GENERAL' },
    { key: 'centro-informacion', label: 'Centro de Información', grupo: 'GENERAL' },

    // SST / RRHH
    { key: 'rrhh-seguimientos', label: 'Seguimientos SST', grupo: 'GENERAL' },

    // CALIDAD
    { key: 'calidad-cyreles', label: 'Cyreles', grupo: 'CALIDAD' },
    { key: 'calidad-formatos', label: 'Formatos', grupo: 'CALIDAD' },
    { key: 'extrusion', label: 'Extrusión', grupo: 'CALIDAD' },
    { key: 'impresion', label: 'Impresión', grupo: 'CALIDAD' },
    { key: 'sellado', label: 'Sellado', grupo: 'CALIDAD' },
    { key: 'precorte', label: 'Precorte', grupo: 'CALIDAD' },
    { key: 'opciones-formulario', label: 'Opciones de Formularios', grupo: 'CALIDAD' },
    { key: 'mejor-rendimiento', label: 'Mejor Rendimiento', grupo: 'CALIDAD' },
    { key: 'gestion-snc', label: 'Gestión S.N.C.', grupo: 'CALIDAD' },

    // CONTROL DE ENVÍOS
    { key: 'trazabilidad', label: 'Trazabilidad', grupo: 'CONTROL DE ENVIOS' },
    { key: 'costos-flete', label: 'Costos Flete', grupo: 'CONTROL DE ENVIOS' },
    { key: 'pedidos', label: 'Pedidos', grupo: 'CONTROL DE ENVIOS' },

    // REPORTES
    { key: 'auditoria', label: 'Auditoría', grupo: 'GENERAL' },

    // CONFIGURACIÓN
    { key: 'configuracion', label: 'Configuración', grupo: 'GENERAL' },
    { key: 'contactos-notificacion', label: 'Contactos', grupo: 'GENERAL' },
    { key: 'vinculaciones-flotachat', label: 'Vincular FlotaChat', grupo: 'GENERAL' },
    { key: 'usuarios', label: 'Usuarios', grupo: 'GENERAL' },

    // COMPRAS Y MATERIALES
    { key: 'proveedores-materiales', label: 'Prov - Materiales', grupo: 'COMPRAS Y MATERIALES' },
    { key: 'orden-compra', label: 'Ord - Compra', grupo: 'COMPRAS Y MATERIALES' },
    { key: 'recepcion-mercancia', label: 'Re - Mercancía', grupo: 'COMPRAS Y MATERIALES' },
    { key: 'inventario', label: 'Inventario', grupo: 'COMPRAS Y MATERIALES' },
    { key: 'traslados', label: 'Traslados', grupo: 'COMPRAS Y MATERIALES' },

    // COMPRAS NO FORMALIZADAS
    { key: 'proveedores-no-formalizados', label: 'Proveedores', grupo: 'COMPRAS NO FORMALIZADAS' },
    { key: 'materiales-no-formalizados', label: 'Materiales', grupo: 'COMPRAS NO FORMALIZADAS' },
    { key: 'ordenes-compras-no-formalizadas', label: 'Ord - Compras', grupo: 'COMPRAS NO FORMALIZADAS' },
    { key: 'recepcion-compras-no-formalizadas', label: 'Rep - Mercancías', grupo: 'COMPRAS NO FORMALIZADAS' },
    { key: 'inventario-compras-no-formalizadas', label: 'Inventario', grupo: 'COMPRAS NO FORMALIZADAS' },
    { key: 'traslado-compras-no-formalizadas', label: 'Traslados', grupo: 'COMPRAS NO FORMALIZADAS' }
  ];

  gruposModulos: any[] = [];

  private construirGruposModulos(): void {
    const grupos: any[] = [];

    for (const modulo of this.modulos) {
      let grupo = grupos.find(g => g.nombre === modulo.grupo);

      if (!grupo) {
        grupo = {
          nombre: modulo.grupo,
          modulos: []
        };

        grupos.push(grupo);
      }

      grupo.modulos.push(modulo);
    }

    this.gruposModulos = grupos;
  }

  trackByGrupo(index: number, grupo: any): string {
    return grupo.nombre;
  }

  trackByModulo(index: number, modulo: any): string {
    return modulo.key;
  }

  getNombreGrupo(nombre: string): string {
    const nombres: Record<string, string> = {
      'GENERAL': 'GENERAL',
      'CONTROL DE ENVIOS': 'CONTROL DE ENVÍOS',
      'COMPRAS Y MATERIALES': 'COMPRAS Y MATERIALES',
      'COMPRAS NO FORMALIZADAS': 'COMPRAS NO FORMALIZADAS',
      'CALIDAD': 'CALIDAD'
    };

    return nombres[nombre] ?? nombre;
  }

  readonly roles = [
    'Admin',
    'Auxiliar',
    'Conductor',
    'Jefe',
    'Facturacion',
    'Bodega',
    'Porteria',
    'RecursosHumanos',
    'PESV',
    'Vendedor',
    'Calidad',
    'Impresion',
    'SST',
    'Compras',
    'Precorte',
    'Extrusion',
    'Sellado',
    'Produccion'
  ];

  constructor(
    private usuariosService: UsuariosService
  ) { }

  ngOnInit(): void {
    this.construirGruposModulos();
    this.cargarUsuarios();
  }

  cargarUsuarios(): void {
    this.usuariosService
      .obtenerUsuarios(
        this.paginaActual,
        this.porPagina,
        this.buscar
      )
      .subscribe({
        next: (resp: any) => {
          if (Array.isArray(resp)) {
            this.usuarios = resp;
            this.totalUsuarios = resp.length;
            this.totalRegistros = resp.length;
            this.totalPaginas = 1;
          } else {
            this.usuarios = resp.data ?? [];
            this.totalRegistros = resp.total ?? 0;
            this.totalUsuarios = resp.total ?? 0;
            this.totalPaginas = resp.totalPaginas ?? 1;
          }
        },
        error: (err: any) => {
          console.error(err);
        }
      });
  }

  buscarUsuarios(): void {
    this.paginaActual = 1;
    this.cargarUsuarios();
  }

  paginaAnterior(): void {
    if (this.paginaActual > 1) {
      this.paginaActual--;
      this.cargarUsuarios();
    }
  }

  paginaSiguiente(): void {
    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
      this.cargarUsuarios();
    }
  }

  get totalActivos(): number {
    return this.usuarios.filter(
      u => u.activo
    ).length;
  }

  getPermiso(modulo: string): PermisoGranular {
    return this.nuevoUsuario.permisos.find(
      p => p.modulo === modulo
    ) ?? {
      modulo,
      puedeVer: false,
      puedeCrear: false,
      puedeEditar: false,
      puedeEliminar: false,
      puedeEnviarCorreo: false,
      puedeVerDatosNumericos: false
    };
  }

  tienePermiso(modulo: string): boolean {
    return this.nuevoUsuario.permisos.some(
      p => p.modulo === modulo
    );
  }

  toggleModulo(modulo: string): void {
    const idx = this.nuevoUsuario.permisos.findIndex(
      p => p.modulo === modulo
    );

    if (idx >= 0) {
      this.nuevoUsuario.permisos.splice(idx, 1);
    } else {
      this.nuevoUsuario.permisos.push({
        modulo,
        puedeVer: true,
        puedeCrear: false,
        puedeEditar: false,
        puedeEliminar: false,
        puedeEnviarCorreo: false,
        puedeVerDatosNumericos: false
      });
    }
  }

  toggleAccion(
    modulo: string,
    accion:
      | 'puedeVer'
      | 'puedeCrear'
      | 'puedeEditar'
      | 'puedeEliminar'
      | 'puedeEnviarCorreo'
      | 'puedeVerDatosNumericos'
  ): void {
    const p = this.nuevoUsuario.permisos.find(
      p => p.modulo === modulo
    );

    if (!p) return;

    p[accion] = !p[accion];

    if (
      accion === 'puedeVer' &&
      !p.puedeVer
    ) {
      p.puedeCrear = false;
      p.puedeEditar = false;
      p.puedeEliminar = false;
      p.puedeEnviarCorreo = false;
      p.puedeVerDatosNumericos = false;
    }

    if (
      accion === 'puedeCrear' ||
      accion === 'puedeEditar' ||
      accion === 'puedeEliminar' ||
      accion === 'puedeEnviarCorreo' ||
      (
        accion === 'puedeVerDatosNumericos' &&
        p[accion]
      )
    ) {
      p.puedeVer = true;
    }
  }

  seleccionarTodos(): void {
    this.nuevoUsuario.permisos =
      this.modulos.map(m => ({
        modulo: m.key,
        puedeVer: true,
        puedeCrear: true,
        puedeEditar: true,
        puedeEliminar: true,
        puedeEnviarCorreo: true,
        puedeVerDatosNumericos: true
      }));
  }

  soloLectura(): void {
    this.nuevoUsuario.permisos =
      this.modulos.map(m => ({
        modulo: m.key,
        puedeVer: true,
        puedeCrear: false,
        puedeEditar: false,
        puedeEliminar: false,
        puedeEnviarCorreo: false,
        puedeVerDatosNumericos: false
      }));
  }

  deseleccionarTodos(): void {
    this.nuevoUsuario.permisos = [];
  }

  agregarUsuario(): void {
    this.editando = false;
    this.usuarioEditarId = null;

    this.nuevoUsuario = {
      username: '',
      password: '',
      rol: 'Auxiliar',
      email: '',
      activo: true,
      permisos: []
    };

    this.passwordConductores = '';
    this.mostrarModal = true;
  }

  guardarUsuario(): void {
    if (!this.nuevoUsuario.username) {
      alert('Ingrese el nombre de usuario');
      return;
    }

    if (
      !this.editando &&
      !this.nuevoUsuario.password
    ) {
      alert('Ingrese la contraseña');
      return;
    }

    if (!this.nuevoUsuario.email) {
      alert('Ingrese el correo electrónico');
      return;
    }

    const peticion = this.editando
      ? this.usuariosService.actualizarUsuario(
        this.usuarioEditarId!,
        this.nuevoUsuario
      )
      : this.usuariosService.crearUsuario(
        this.nuevoUsuario
      );

    peticion.subscribe({
      next: () => {
        this.cargarUsuarios();
        this.cerrarModal();
      },
      error: (err: any) => {
        console.error(err);
        alert(
          err.error ||
          'Error guardando usuario'
        );
      }
    });
  }

  editarUsuario(usuario: any): void {
    this.editando = true;
    this.usuarioEditarId = usuario.id;

    const permisos: PermisoGranular[] =
      (usuario.permisos ?? []).map(
        (p: any) => ({
          modulo: p.modulo,
          puedeVer: p.puedeVer ?? true,
          puedeCrear: p.puedeCrear ?? false,
          puedeEditar: p.puedeEditar ?? false,
          puedeEliminar: p.puedeEliminar ?? false,
          puedeEnviarCorreo:
            p.puedeEnviarCorreo ?? false,
          puedeVerDatosNumericos:
            p.puedeVerDatosNumericos ?? false
        })
      );

    this.nuevoUsuario = {
      username: usuario.username,
      password: '',
      rol: usuario.rol,
      email: usuario.email ?? '',
      activo: usuario.activo,
      permisos
    };

    this.mostrarModal = true;
  }

  eliminarUsuario(id: number): void {
    if (!confirm('¿Eliminar usuario?')) return;

    this.usuariosService
      .eliminarUsuario(id)
      .subscribe({
        next: () => this.cargarUsuarios(),
        error: (err: any) => {
          console.error(err);
        }
      });
  }

  cambiarEstado(usuario: any): void {
    this.usuariosService
      .cambiarEstado(usuario.id)
      .subscribe({
        next: () => this.cargarUsuarios(),
        error: (err: any) => {
          console.error(err);
        }
      });
  }

  abrirRecuperar(): void {
    this.emailRecuperar = '';
    this.tokenRecuperar = '';
    this.nuevaPassword = '';
    this.tokenGenerado = '';
    this.mensajeRecuperar = '';
    this.mostrarRecuperar = true;
    this.mostrarCambiar = false;
  }

  solicitarRecuperacion(): void {
    if (!this.emailRecuperar) {
      alert('Ingrese el correo');
      return;
    }

    this.usuariosService
      .solicitarRecuperacion(
        this.emailRecuperar
      )
      .subscribe({
        next: (data: any) => {
          this.tokenGenerado = data.token;
          this.mensajeRecuperar =
            `Token: ${data.token}`;
          this.mostrarCambiar = true;
        },
        error: (err: any) => {
          alert(
            err.error ||
            'Error solicitando recuperación'
          );
        }
      });
  }

  cambiarPassword(): void {
    if (!this.tokenRecuperar) {
      alert('Ingrese el token');
      return;
    }

    if (!this.nuevaPassword) {
      alert('Ingrese la nueva contraseña');
      return;
    }

    this.usuariosService
      .cambiarPassword(
        this.emailRecuperar,
        this.tokenRecuperar,
        this.nuevaPassword
      )
      .subscribe({
        next: () => {
          alert(
            'Contraseña cambiada correctamente'
          );

          this.mostrarRecuperar = false;
          this.mostrarCambiar = false;
        },
        error: (err: any) => {
          alert(
            err.error ||
            'Token inválido o expirado'
          );
        }
      });
  }

  cerrarModal(): void {
    this.mostrarModal = false;
    this.mostrarRecuperar = false;
    this.mostrarCambiar = false;
  }

  getBadgeRol(rol: string): string {
    switch (rol) {
      case 'Admin':
        return 'badge-admin';
      case 'Auxiliar':
        return 'badge-auxiliar';
      case 'Conductor':
        return 'badge-conductor';
      case 'Jefe':
        return 'badge-jefe';
      case 'Facturacion':
        return 'badge-facturacion';
      case 'Bodega':
        return 'badge-bodega';
      case 'Porteria':
        return 'badge-porteria';
      case 'RecursosHumanos':
        return 'badge-rrhh';
      case 'PESV':
        return 'badge-pesv';
      case 'Vendedor':
        return 'badge-vendedor';
      case 'Calidad':
        return 'badge-calidad';
      case 'Impresion':
        return 'badge-impresion';
      case 'Compras':
        return 'badge-compras';
      case 'Precorte':
        return 'badge-precorte';
      case 'Extrusion':
        return 'badge-extrusion';
      case 'Sellado':
        return 'badge-sellado';
      case 'Gestion-snc':
        return 'badge-gestion-snc';
      case 'Produccion':
        return 'badge-produccion';
      default:
        return 'badge-auxiliar';
    }
  }
}

interface PermisoGranular {
  modulo: string;
  puedeVer: boolean;
  puedeCrear: boolean;
  puedeEditar: boolean;
  puedeEliminar: boolean;
  puedeEnviarCorreo: boolean;
  puedeVerDatosNumericos: boolean;
}