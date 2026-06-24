import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule }  from '@angular/forms';
import { AuthService }  from '../../services/auth.service';
import { ConfiguracionService } from '../../services/configuracion.service';
import { environment } from '../../../environments/environment';
import { retry } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent implements OnInit {

  @Output() loginSuccess = new EventEmitter<any>();

  username = '';
  password = '';
  cargando         = false;
  error             = '';
  mostrarPassword   = false;

  // ── Datos de la empresa ───────────────────────────────────────────────────
  nombreEmpresa = 'Sistema de Flota';
  logoUrl: string | null = null;

  // ── Bienvenida post-login ─────────────────────────────────────────────────
  mostrarBienvenida = false;
  nombreUsuario     = '';
  readonly baseUrl  = environment.fotosUrl;

  constructor(
    private auth: AuthService,
    private configuracionService: ConfiguracionService
  ) {}

  ngOnInit(): void {
    this.cargarEmpresa();
  }

  cargarEmpresa() {
    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        if (data.nombreEmpresa) this.nombreEmpresa = data.nombreEmpresa;
        if (data.logo)          this.logoUrl = `${this.baseUrl}/config/${data.logo}`;
        if (data.colorCorporativo)
          document.documentElement.style.setProperty('--color-primario', data.colorCorporativo);
      },
      error: () => {} // Si no hay config, usa valores por defecto
    });
  }

  iniciarSesion() {
    if (!this.username || !this.password) {
      this.error = 'Ingrese usuario y contraseña';
      return;
    }

    this.cargando = true;
    this.error    = '';

    this.auth.login(this.username, this.password).pipe(
    retry({ count: 2, delay: 2000 }) ).subscribe({
      next: (res: any) => {
        localStorage.setItem('token', res.token);
        localStorage.setItem('user', JSON.stringify({
          username: res.username,
          rol:      res.rol,
          email:    res.email,
          permisos: res.permisos ?? []
        }));

        // Mostrar pantalla de bienvenida 2 segundos antes de entrar
        this.nombreUsuario     = res.username;
        this.mostrarBienvenida = true;
        this.cargando          = false;

        setTimeout(() => {
          this.loginSuccess.emit(res);
        }, 2500);
      },
      error: (err) => {
        this.cargando = false;
        if (err.status === 429) {
          this.error = err.error?.mensaje ?? 'Demasiados intentos. Espera unos minutos.';
        } else {
          this.error = 'Usuario o contraseña incorrectos';
        }
      }
    });
  }
}