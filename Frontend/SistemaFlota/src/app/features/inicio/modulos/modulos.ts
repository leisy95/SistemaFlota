import { Component, EventEmitter, Output } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { Dialog } from '../../../shared/reutilizable/dialog/dialog';

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
        permitido = this.permisos.some(p =>
          [
            'inspecciones',
            'conductores',
            'vehiculos',
            'reporte-ruta',
            'autorizaciones',
            'documentos',
            'mantenimiento',
            'checklist',
            'historial'
          ].includes(p.modulo) && p.puedeVer);
        break;

      case '/rrhh':
        nombreModulo = 'Recursos Humanos';
        permitido = this.permisos.some(p =>
          p.modulo === 'rrhh-seguimientos' && p.puedeVer);
        break;

      case '/calidad':
        nombreModulo = 'Calidad';
        permitido = this.permisos.some(p =>
          p.modulo.startsWith('calidad-') && p.puedeVer);
        break;

      case '/control-envios':
        nombreModulo = 'Control de Envíos';
        permitido = this.permisos.some(p =>
          [
            'trazabilidad',
            'pedidos',
            'costos-flete'
          ].includes(p.modulo) && p.puedeVer);
        break;

      case '/reportes':
        nombreModulo = 'Reportes';
        permitido = this.permisos.some(p =>
          p.modulo === 'auditoria' && p.puedeVer);
        break;

      case '/configuracion':
        nombreModulo = 'Configuración';
        permitido = this.permisos.some(p =>
          [
            'configuracion',
            'usuarios',
            'contactos'
          ].includes(p.modulo) && p.puedeVer);
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