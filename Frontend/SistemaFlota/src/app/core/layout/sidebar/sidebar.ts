import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';

import { AuthService } from '../../services/auth.service';
import { ConfiguracionService } from '../../services/configuracion.service';
import { environment } from '../../../../environments/environment';
import { MENU_MODULOS } from '../../menu.config'; import { filter } from 'rxjs';
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.scss',
})
export class Sidebar implements OnInit {

  @Input() abierto = false;
  @Output() cerrar = new EventEmitter<void>();

  usuario = '';
  rol = '';
  permisos: any[] = [];

  empresaNombre = 'Flota';
  empresaLogo: string | null = null; modulosVisibles = MENU_MODULOS;

  moduloActivo = ''; private readonly baseUrl = environment.fotosUrl;

  constructor(
    private authService: AuthService,
    private router: Router,
    private configuracionService: ConfiguracionService
  ) { }

  ngOnInit(): void {
    const sesion = this.authService.obtenerUsuarioActual();

    if (sesion) {
      this.usuario = sesion.username;
      this.rol = sesion.rol;
      this.permisos = sesion.permisos ?? [];
    }

    this.cargarConfiguracion();
    this.filtrarMenu();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.filtrarMenu();
      });
  }

  cambiarModulo(ruta: string): void {
    this.moduloActivo = ruta;
    this.router.navigate([ruta]);
    this.cerrar.emit();
  }

  cargarConfiguracion(): void {
    this.configuracionService.obtenerConfiguracion()
      .subscribe({

        next: (data: any) => {
          if (data.nombreEmpresa?.trim()) {

            this.empresaNombre = data.nombreEmpresa;

          } if (data.logo?.trim()) {

            this.empresaLogo =
              `${this.baseUrl}/config/${data.logo}`;

          }

        },

        error: () => { }

      });
  }

  private obtenerMenuPorModulo(modulo: string) {

  if (this.rol === 'Admin') {
    return MENU_MODULOS.filter(x => x.modulo === modulo);
  }

  const menu = MENU_MODULOS
    .filter(x => x.modulo === modulo)
    .filter(menu =>
      this.permisos.some(p =>
        p.modulo === menu.key &&
        p.puedeVer
      )
    );

  return menu;
}

  // Para mostrar el menu lateral
  filtrarMenu(): void {

    const url = this.router.url;

    if (url.startsWith('/flota')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('flota');
    }
    else if (url.startsWith('/rrhh')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('rrhh');
    }
    else if (url.startsWith('/calidad')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('calidad');
    }
    else if (url.startsWith('/control-envios')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('control-envios');
    }
    else if (url.startsWith('/reportes')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('reportes');
    }
    else if (url.startsWith('/configuracion')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('configuracion');
    }
    else if (url.startsWith('/costos')) {
      this.modulosVisibles = this.obtenerMenuPorModulo('costos');
    }
    else {
      this.modulosVisibles = [];
    }

    this.moduloActivo = url;

  }

  irAlInicio(): void {
    this.router.navigate(['/inicio']);
  }
}