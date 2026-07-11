import { Component, OnInit } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { FormsModule }       from '@angular/forms';
import { ChecklistService } from '../../../core/services/checklist.service';
import { TiposVehiculoService } from '../../../core/services/tipos-vehiculo.service';

@Component({
  selector: 'app-checklist-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './checklist-editor.html',
  styleUrls: ['./checklist-editor.scss']
})
export class ChecklistEditorComponent implements OnInit {

  todosChecklist: any[] = [];
  checklistPaginado: any[] = [];
  tiposVehiculo:  any[] = [];

  descripcion    = '';
  tipoVehiculoId = 0;
  editando       = false;
  editandoId     = 0;
  paginaActual   = 1;
  itemsPorPagina = 10;

  constructor(
    private checklistService:     ChecklistService,
    private tiposVehiculoService: TiposVehiculoService
  ) {}

  ngOnInit(): void {
    this.obtenerTipos();
    this.obtenerTodos();
  }

  obtenerTipos() {
    this.tiposVehiculoService.obtenerTipos().subscribe({
      next: (data) => this.tiposVehiculo = data,
      error: (err)  => console.error(err)
    });
  }

  obtenerTodos() {
    this.checklistService.obtenerTodos().subscribe({
      next: (data) => { this.todosChecklist = data; this.actualizarPaginado(); },
      error: (err)  => console.error(err)
    });
  }

  // ── Nombre del tipo de vehículo ───────────────────────────────────────────
  getNombreTipo(tipoVehiculoId: number): string {
    const tipo = this.tiposVehiculo.find(t => t.id === tipoVehiculoId);
    return tipo?.nombre ?? `Tipo ${tipoVehiculoId}`;
  }

  // ── Paginación como propiedad ─────────────────────────────────────────────
  private actualizarPaginado() {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    this.checklistPaginado = this.todosChecklist.slice(inicio, inicio + this.itemsPorPagina);
  }

  get totalPaginas(): number {
    return Math.ceil(this.todosChecklist.length / this.itemsPorPagina);
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) { this.paginaActual++; this.actualizarPaginado(); }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) { this.paginaActual--; this.actualizarPaginado(); }
  }

  // ── CRUD ──────────────────────────────────────────────────────────────────
  guardarChecklist() {
    if (this.tipoVehiculoId === 0) { alert('Seleccione tipo vehículo'); return; }
    if (!this.descripcion.trim())  { alert('Ingrese descripción');       return; }

    const data = { descripcion: this.descripcion, tipoVehiculoId: this.tipoVehiculoId };

    if (this.editando) {
      this.checklistService.editarChecklist(this.editandoId, data).subscribe({
        next: () => { this.limpiarFormulario(); this.obtenerTodos(); },
        error: (err) => { console.error(err); alert('Error al actualizar'); }
      });
    } else {
      this.checklistService.crearChecklist(data).subscribe({
        next: () => { this.limpiarFormulario(); this.obtenerTodos(); },
        error: (err) => { console.error(err); alert('Error al guardar'); }
      });
    }
  }

  editarChecklist(item: any) {
    this.editando      = true;
    this.editandoId    = item.id;
    this.descripcion   = item.descripcion;
    this.tipoVehiculoId = item.tipoVehiculoId;
  }

  eliminarChecklist(id: number) {
    if (!confirm('¿Eliminar checklist?')) return;
    this.checklistService.eliminarChecklist(id).subscribe({
      next: () => this.obtenerTodos(),
      error: (err) => { console.error(err); alert('Error al eliminar'); }
    });
  }

  limpiarFormulario() {
    this.descripcion = ''; this.tipoVehiculoId = 0;
    this.editando = false; this.editandoId = 0;
  }
}