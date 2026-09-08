import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FormatosCalidadService } from '../../../core/services/formatos-calidad.service';
import { OpcionesFormularioService } from '../../../core/services/opciones-formulario.service';

@Component({
  selector: 'app-mejor-rendimiento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mejor-rendimiento.html',
  styleUrls: ['./mejor-rendimiento.scss']
})
export class MejorRendimientoComponent implements OnInit {
  referenciaBusqueda = '';
  maquinaBusqueda = '';
  opcionesMaquina: any[] = [];
  buscando = false;
  resultado: any = null;
  mensajeVacio = '';

  constructor(
    private service: FormatosCalidadService,
    private opcionesService: OpcionesFormularioService
  ) { }

  ngOnInit(): void {
    this.cargarOpcionesMaquina();
  }

  cargarOpcionesMaquina() {
    this.opcionesService.getOpciones('Maquina').subscribe({
      next: (d) => this.opcionesMaquina = d,
      error: (e) => console.error('Error cargando máquinas', e)
    });
  }

  buscar() {
    if (!this.referenciaBusqueda.trim()) { alert('Ingrese orden, referencia o cliente'); return; }

    this.buscando = true;
    this.resultado = null;
    this.mensajeVacio = '';

    this.service.buscarMejorRendimiento(this.referenciaBusqueda.trim(), this.maquinaBusqueda || undefined).subscribe({
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