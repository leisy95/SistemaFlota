import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { NavigationEnd, Router } from '@angular/router';
import { MENU_MODULOS } from '../../menu.config';
import { filter } from 'rxjs';

@Component({
  selector: 'app-topbar',
  imports: [],
  templateUrl: './topbar.html',
  styleUrl: './topbar.scss',
})
export class Topbar implements OnInit {

  @Output() logout = new EventEmitter<void>();
  @Output() toggleSidebar = new EventEmitter<void>();

  usuario = '';
  rol = '';

  nombreModulo = 'Panel de control';
  iconoModulo = 'fa-solid fa-gauge-high';

  constructor(
    public authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {

    const sesion = this.authService.obtenerUsuarioActual();

    if (sesion) {
      this.usuario = sesion.username;
      this.rol = sesion.rol;
    }

    this.actualizarModulo();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => this.actualizarModulo());

  }

  actualizarModulo(): void {

    const ruta = this.router.url;

    const menu = MENU_MODULOS.find(m => m.ruta === ruta);

    if (menu) {
      this.nombreModulo = menu.label;
      this.iconoModulo = menu.icon;
    } else {
      this.nombreModulo = 'Panel de control';
      this.iconoModulo = 'fa-solid fa-gauge-high';
    }

  }

  cerrarSesion(): void {

    this.authService.logout(
      this.authService.username,
      this.authService.rol
    ).subscribe({
      next: () => {
        this.authService.limpiarSesion();
        this.router.navigate(['/']);
      },
      error: () => {
        // Aunque falle el logout del servidor,
        // se cierra la sesión local.
        this.authService.limpiarSesion();
        this.router.navigate(['/']);
      }
    });

  }
}
