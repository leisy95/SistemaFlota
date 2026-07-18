import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VinculacionesFlotaChatService } from '../../../core/services/vinculaciones-flotachat.service';

@Component({
  selector: 'app-vinculaciones-flotachat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vinculaciones-flotachat.html',
  styleUrls: ['./vinculaciones-flotachat.scss']
})
export class VinculacionesFlotaChatComponent implements OnInit {
  tipoActivo: 'Conductor' | 'ContactoNotificacion' = 'Conductor';

  pendientes: any[] = [];
  vinculados: any[] = [];
  entidades: any[] = [];
  cargando = false;

  seleccionManual: { [usuarioId: number]: number | null } = {};

  constructor(private service: VinculacionesFlotaChatService) {}

  ngOnInit(): void {
    this.cargarTodo();
  }

  cambiarTipo(tipo: 'Conductor' | 'ContactoNotificacion') {
    this.tipoActivo = tipo;
    this.seleccionManual = {};
    this.cargarTodo();
  }

  cargarTodo() {
    this.cargando = true;

    this.service.obtenerEntidades(this.tipoActivo).subscribe({
      next: (data) => this.entidades = data,
      error: (err) => console.error(err)
    });

    this.service.obtenerVinculadas(this.tipoActivo).subscribe({
      next: (data) => this.vinculados = data,
      error: (err) => console.error(err)
    });

    this.service.obtenerPendientes(this.tipoActivo).subscribe({
      next: (data) => { this.pendientes = data; this.cargando = false; },
      error: (err) => { console.error(err); this.cargando = false; }
    });
  }

  nombreEntidad(entidadId: number): string {
    const e = this.entidades.find(e => e.id === entidadId);
    return e ? e.nombre : `#${entidadId}`;
  }

  vincular(pendiente: any) {
    const entidadId = pendiente.sugerencia?.id ?? this.seleccionManual[pendiente.id];
    if (!entidadId) { alert('Seleccione una opción'); return; }

    this.service.vincular(pendiente.id, this.tipoActivo, entidadId, pendiente.celular).subscribe({
      next: () => this.cargarTodo(),
      error: (err) => { console.error(err); alert('Error al vincular'); }
    });
  }

  desvincular(id: number) {
    if (!confirm('¿Quitar esta vinculación?')) return;
    this.service.eliminar(id).subscribe({
      next: () => this.cargarTodo(),
      error: (err) => console.error(err)
    });
  }
}