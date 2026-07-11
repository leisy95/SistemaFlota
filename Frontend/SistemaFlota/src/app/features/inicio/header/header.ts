import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {

  @Output() logout = new EventEmitter<void>();

  usuario = '';
  rol = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {

    const sesion = this.authService.obtenerUsuarioActual();

    if (sesion) {

      this.usuario = sesion.username;
      this.rol = sesion.rol;

    }
  }
  cerrarSesion() {

    this.authService.logout(
      this.usuario,
      this.rol
    ).subscribe();

    localStorage.clear();
    sessionStorage.clear();
    this.router.navigate(['/']);
  }

}
