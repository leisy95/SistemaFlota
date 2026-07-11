import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConfiguracionService } from './core/services/configuracion.service';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet
  ],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class AppComponent implements OnInit {

  constructor(
    private configuracionService: ConfiguracionService,
    private authService: AuthService,
    private router: Router
  ) { }

  ngOnInit(): void {

    this.cargarConfiguracion();

    const usuario = this.authService.obtenerUsuarioActual();

    if (!usuario) {
      this.router.navigate(['/']);
    }

  }

  cargarConfiguracion() {

    this.configuracionService.obtenerConfiguracion()
      .subscribe({

        next: (data: any) => {

        },

        error: () => { }

      });

  }
}