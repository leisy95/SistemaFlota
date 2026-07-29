import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormatosCalidadService } from '../../../core/services/formatos-calidad.service';

@Component({
  selector: 'app-mejor-rendimiento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mejor-rendimiento.html',
  styleUrls: ['./mejor-rendimiento.scss']
})
export class MejorRendimientoComponent {
  referenciaBusqueda = '';
  buscando = false;
  resultado: any = null;
  mensajeVacio = '';

  constructor(private service: FormatosCalidadService) {}

  buscar() {
    if (!this.referenciaBusqueda.trim()) { alert('Ingrese una referencia'); return; }

    this.buscando = true;
    this.resultado = null;
    this.mensajeVacio = '';

    this.service.buscarMejorRendimiento(this.referenciaBusqueda.trim()).subscribe({
      next: (data: any) => {
        this.buscando = false;
        if (!data.mejor) { this.mensajeVacio = data.mensaje || 'No hay registros'; return; }
        this.resultado = data;
        this.seleccionado = data.mejor;
      },
      error: (e) => {
        this.buscando = false;
        this.mensajeVacio = 'Error al buscar';
        console.error(e);
      }
    });
  }

  variablesDe(json: string | null): any {
    if (!json) return null;
    try { return JSON.parse(json); } catch { return null; }
  }

  seleccionado: any = null;

  seleccionar(r: any) {
    this.seleccionado = r;
  }
}