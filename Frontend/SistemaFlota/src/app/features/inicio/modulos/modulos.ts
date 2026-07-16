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

      case '/flota':

        nombreModulo = 'Flota';

        const modulosFlota = MENU_MODULOS
          .filter(m => m.modulo === 'flota')
          .map(m => m.key);

        permitido = this.permisos.some(p =>
          p.puedeVer && modulosFlota.includes(p.modulo)
        );

        if (permitido) {

          const inicio = this.permisos.find(p =>
            p.puedeVer && p.esInicio
          );

          if (inicio) {
            this.router.navigate(['/flota', inicio.modulo]);
            return;
          }

          const primero = this.permisos.find(p =>
            p.puedeVer && modulosFlota.includes(p.modulo)
          );

          if (primero) {
            this.router.navigate(['/flota', primero.modulo]);
            return;
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

  cerrarDialog(): void {
    this.dialogVisible = false;
  }

}