import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Header } from './header/header';
import { Resumen } from './resumen/resumen';
import { Modulos } from './modulos/modulos';
import { Actividades } from './actividades/actividades';

@Component({
  selector: 'app-inicio',
  standalone: true,
  imports: [
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

  cambiarModulo(modulo: string) {

    this.navegar.emit(modulo);
  }

  cerrarSesion() {
    this.logout.emit();
  }

}
