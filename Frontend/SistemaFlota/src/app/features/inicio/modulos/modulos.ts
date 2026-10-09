import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Dialog } from '../../../shared/reutilizable/dialog/dialog';
import { MENU_MODULOS } from '../../../core/menu.config';

@Component({
  selector: 'app-modulos',
  standalone: true,
  imports: [
    Dialog
  ],
  templateUrl: './modulos.html',
  styleUrl: './modulos.scss',
})
export class Modulos {

  permisos: any[] = [];
  rol = '';

  dialogVisible = false;
  dialogTitulo = '';
  dialogMensaje = '';
  dialogTipo: 'success' | 'warning' | 'error' | 'info' = 'warning';

  constructor(
    private router: Router,
    private authService: AuthService
  ) {

    const sesion = this.authService.obtenerUsuarioActual();

    this.permisos = sesion?.permisos ?? [];
    this.rol = sesion?.rol ?? '';

  }

  navegarModulo(ruta: string) {

    if (this.rol === 'Admin') {
      this.router.navigate([ruta]);
      return;
    }

    let permitido = false;
    let nombreModulo = '';

    switch (ruta) {

      case '/flota': {
        nombreModulo = 'Flota';

        const modulosFlota = MENU_MODULOS.filter(
          m => m.modulo === 'flota'
        );

        const permisosFlota = this.permisos.filter(
          p => p.puedeVer &&
            modulosFlota.some(m => m.key === p.modulo)
        );

        permitido = permisosFlota.length > 0;

        if (permitido) {
          const permisoInicio = permisosFlota.find(p => p.esInicio);
          const permisoDestino = permisoInicio ?? permisosFlota[0];

          const menuDestino = modulosFlota.find(
            m => m.key === permisoDestino.modulo
          );

          if (menuDestino) {
            this.router.navigate([menuDestino.ruta]);
            return;
          }

          permitido = false;
        }

        break;
      }

      case '/costos':
        nombreModulo = 'Costos';

        const modulosCostos = MENU_MODULOS
          .filter(m => m.modulo === 'costos')
          .map(m => m.key);

        permitido = this.permisos.some(p =>
          p.puedeVer &&
          modulosCostos.includes(p.modulo)
        );

        if (permitido) {

          const inicio = this.permisos.find(p =>
            p.puedeVer &&
            p.esInicio &&
            modulosCostos.includes(p.modulo)
          );

          if (inicio) {

            const menuInicio = MENU_MODULOS.find(m =>
              m.modulo === 'costos' &&
              m.key === inicio.modulo
            );

            if (menuInicio) {
              this.router.navigate([menuInicio.ruta]);
              return;
            }
          }

          const primero = this.permisos.find(p =>
            p.puedeVer &&
            modulosCostos.includes(p.modulo)
          );

          if (primero) {

            const menuPrimero = MENU_MODULOS.find(m =>
              m.modulo === 'costos' &&
              m.key === primero.modulo
            );

            if (menuPrimero) {
              this.router.navigate([menuPrimero.ruta]);
              return;
            }
          }
        }

        break;

      case '/rrhh':
        nombreModulo = 'Recursos Humanos';
        permitido = this.permisos.some(p =>
          p.puedeVer &&
          MENU_MODULOS.some(m => m.modulo === 'rrhh' && m.key === p.modulo)
        );
        break;

      case '/calidad':
        nombreModulo = 'Calidad';
        permitido = this.permisos.some(p =>
          p.puedeVer &&
          MENU_MODULOS.some(m => m.modulo === 'calidad' && m.key === p.modulo)
        );
        break;

      case '/control-envios':
        nombreModulo = 'Control de Envíos';
        permitido = this.permisos.some(p =>
          p.puedeVer &&
          MENU_MODULOS.some(m => m.modulo === 'control-envios' && m.key === p.modulo)
        );
        break;

      case '/reportes':
        nombreModulo = 'Reportes';
        permitido = this.permisos.some(p =>
          p.puedeVer &&
          MENU_MODULOS.some(m => m.modulo === 'reportes' && m.key === p.modulo)
        );
        break;

      case '/historial-odo':
      case '/historial-odo/movimiento-producto':
      case '/historial-odo/pedido-compra':

        nombreModulo = 'Historial ODO';

        const menuHistorialOdo = MENU_MODULOS.find(m =>
          m.ruta === ruta
        );

        // Si es la entrada principal /historial-odo,
        // basta con tener permiso para cualquiera de sus submódulos.
        if (ruta === '/historial-odo') {

          const modulosHistorialOdo = MENU_MODULOS
            .filter(m => m.modulo === 'historial-odo')
            .map(m => m.key);

          permitido = this.permisos.some(p =>
            p.puedeVer &&
            modulosHistorialOdo.includes(p.modulo)
          );

        } else {

          // Para un submódulo se valida exactamente su permiso.
          permitido = !!menuHistorialOdo &&
            this.permisos.some(p =>
              p.puedeVer &&
              p.modulo === menuHistorialOdo.key
            );
        }

        break;

      case '/configuracion':
        nombreModulo = 'Configuración';
        permitido = this.permisos.some(p =>
          p.puedeVer &&
          MENU_MODULOS.some(m => m.modulo === 'configuracion' && m.key === p.modulo)
        );
        break;
    }

    if (!permitido) {

      this.dialogTitulo = 'Acceso denegado';
      this.dialogMensaje = `No tienes permisos para acceder al módulo ${nombreModulo}.`;
      this.dialogTipo = 'warning';
      this.dialogVisible = true;

      return;
    }

    this.router.navigate([ruta]);

  }

  puedeVerModulo(modulo: string): boolean {

    if (this.rol === 'Admin') {
      return true;
    }

    const modulosPermitidos = MENU_MODULOS
      .filter(m => m.modulo === modulo)
      .map(m => m.key);

    return this.permisos.some(p =>
      p.puedeVer &&
      modulosPermitidos.includes(p.modulo)
    );
  }

  cantidadSubmodulos(modulo: string): number {

    if (this.rol === 'Admin') {
      return MENU_MODULOS.filter(m => m.modulo === modulo).length;
    }

    const modulosDelSistema = MENU_MODULOS
      .filter(m => m.modulo === modulo)
      .map(m => m.key);

    return this.permisos.filter(p =>
      p.puedeVer &&
      modulosDelSistema.includes(p.modulo)
    ).length;
  }

  cerrarDialog(): void {
    this.dialogVisible = false;
  }

}