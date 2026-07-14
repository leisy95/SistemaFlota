import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ConfiguracionService } from '../../core/services/configuracion.service';
import { environment } from '../../../environments/environment';
import { retry } from 'rxjs';
import { PermisosService } from '../../core/services/permisos.service';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent implements OnInit {


  username = '';
  password = '';

  cargando = false;
  error = '';

  mostrarPassword = false;

  // Empresa
  nombreEmpresa = 'Sistema de Flota';
  logoUrl: string | null = null;

  readonly baseUrl = environment.fotosUrl;

  mostrarBienvenida = false;
  nombreUsuario = '';

  constructor(
    private auth: AuthService,
    private configuracionService: ConfiguracionService,
    private router: Router,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.cargarEmpresa();
  }
  cargarEmpresa() {

    this.configuracionService.obtenerConfiguracion()
      .subscribe({
        next: (data: any) => {

          if (data.nombreEmpresa)
            this.nombreEmpresa = data.nombreEmpresa;

          if (data.logo)
            this.logoUrl = `${this.baseUrl}/config/${data.logo}`;

          if (data.colorCorporativo) {

            document.documentElement.style
              .setProperty(
                '--color-primario',
                data.colorCorporativo
              );
          }
        },
        error: () => { }
      });
  }

  iniciarSesion() {

    if (!this.username || !this.password) {
      this.error = 'Ingrese usuario y contraseña';
      return;
    }

    this.cargando = true;
    this.error = '';

    this.auth.login(this.username, this.password)
      .pipe(
        retry({
          count: 2,
          delay: 2000
        })
      )
      .subscribe({
        next: (res: any) => {

          this.auth.guardarSesion({

            token: res.token,
            username: res.username,
            rol: res.rol,
            email: res.email,
            permisos: res.permisos ?? []

          });

          this.permisosService.cargar({
            rol: res.rol,
            permisos: res.permisos ?? []
          });

          console.log('Después de cargar:', this.permisosService);

          this.nombreUsuario = res.username;
          this.mostrarBienvenida = true;
          this.cargando = false;
          setTimeout(() => {

            this.router.navigate(['/inicio']);
          }, 2500);

        },
        error: (err) => {
          this.cargando = false;
          if (err.status === 429) {
            this.error =
              err.error?.mensaje ??
              'Demasiados intentos. Espera unos minutos.';
          }
          else {
            this.error =
              'Usuario o contraseña incorrectos';
          }
        }
      });
  }
}