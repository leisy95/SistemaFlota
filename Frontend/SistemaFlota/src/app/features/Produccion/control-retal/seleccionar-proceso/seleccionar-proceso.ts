
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

interface Proceso {
  nombre: string;
  descripcion: string;
  icono: string;
  color: string;
}

@Component({
  selector: 'app-seleccionar-proceso',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './seleccionar-proceso.html',
  styleUrl: './seleccionar-proceso.scss'
})
export class SeleccionarProceso {

  procesos: Proceso[] = [
    {
      nombre: 'Extrusión',
      descripcion: 'Registrar retal generado durante el proceso de extrusión.',
      icono: 'fa-solid fa-industry',
      color: 'verde'
    },
    {
      nombre: 'Precorte',
      descripcion: 'Registrar material sobrante del proceso de precorte.',
      icono: 'fa-solid fa-scissors',
      color: 'azul'
    },
    {
      nombre: 'Doblado',
      descripcion: 'Registrar retal generado durante el doblado.',
      icono: 'fa-solid fa-layer-group',
      color: 'naranja'
    },
    {
      nombre: 'Impresión',
      descripcion: 'Registrar sobrantes generados durante la impresión.',
      icono: 'fa-solid fa-print',
      color: 'morado'
    },
    {
      nombre: 'Sellado',
      descripcion: 'Registrar material sobrante del proceso de sellado.',
      icono: 'fa-solid fa-box',
      color: 'rojo'
    },
    {
      nombre: 'Otros procesos',
      descripcion: 'Registrar retal generado en otros procesos de producción.',
      icono: 'fa-solid fa-gears',
      color: 'gris'
    }
  ];

  constructor(private router: Router) { }

  seleccionarProceso(proceso: Proceso): void {
    this.router.navigate(
      ['/produccion/control-retal/registrar-movimientos'],
      {
        queryParams: {
          proceso: proceso.nombre
        }
      }
    );
  }
}
