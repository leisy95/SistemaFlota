import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { Router } from '@angular/router';

import { Header } from './header/header';
import { Resumen } from './resumen/resumen';
import { Modulos } from './modulos/modulos';
import { Actividades } from './actividades/actividades';

import { PermisosService } from '../../core/services/permisos.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-inicio',
  standalone: true,
  imports: [
    CommonModule,
    Header,
    Resumen,
    Modulos,
    Actividades
  ],
  templateUrl: './inicio.html',
  styleUrl: './inicio.scss',
})
export class Inicio {

  @Output() navegar = new EventEmitter<string>();
  @Output() logout = new EventEmitter<void>();

  @Input() usuario = '';
  @Input() rol = '';

  private router = inject(Router);
  private permisosService = inject(PermisosService);

  tienePermiso(modulo: string): boolean {
    return this.permisosService.puedeVer(modulo);
  }

  cambiarModulo(modulo: string) {
    this.navegar.emit(modulo);
  }

  irConductores() {
    this.router.navigate(['/flota', 'conductores']);
  }

  irConfirmarSalida() {
    this.router.navigate(['/flota', 'autorizaciones'], {
      queryParams: { accion: 'salida' }
    });
  }

  irRegistrarLlegada() {
    this.router.navigate(['/flota', 'autorizaciones'], {
      queryParams: { accion: 'llegada' }
    });
  }

  cerrarSesion() {
    this.logout.emit();
  }
}