import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OpcionesFormularioService } from '../../../core/services/opciones-formulario.service';
import { FormatosCalidadService } from '../../../core/services/formatos-calidad.service';
import { PermisosService } from '../../../core/services/permisos.service';

@Component({
  selector: 'app-opciones-formulario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './opciones-formulario.html',
  styleUrls: ['./opciones-formulario.scss']
})
export class OpcionesFormularioComponent implements OnInit {
  esAdmin = false;
  opciones: any[] = [];
  opcionesFiltradas: any[] = [];
  tipos: any[] = [];
  categorias: string[] = ['Maquina', 'Corona', 'Molde', 'Operario'];

  filtroCategoria = '';
  mostrarModal = false;
  editando = false;
  editandoId: number | null = null;

  form = { categoria: '', tipoFormatoId: null as number | null, valor: '', orden: 0 };

  get puedeCrear(): boolean { return this.permisosService.puedeCrear('calidad-formatos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('calidad-formatos'); }

  constructor(
    private service: OpcionesFormularioService,
    private formatosService: FormatosCalidadService,
    private permisosService: PermisosService
  ) {}

  ngOnInit(): void {
    const user = JSON.parse(sessionStorage.getItem('user') || '{}');
    this.esAdmin = user.rol === 'Admin';
    if (!this.esAdmin) return;

    this.cargarTipos();
    this.cargar();
  }

  cargarTipos() {
    this.formatosService.getTipos().subscribe({
      next: (d) => this.tipos = d,
      error: (e) => console.error(e)
    });
  }

  cargar() {
    this.service.getTodas().subscribe({
      next: (d) => { this.opciones = d; this.aplicarFiltro(); },
      error: (e) => console.error(e)
    });
  }

  aplicarFiltro() {
    this.opcionesFiltradas = this.filtroCategoria
      ? this.opciones.filter(o => o.categoria === this.filtroCategoria)
      : this.opciones;
  }

  nombreTipo(tipoFormatoId: number | null): string {
    if (!tipoFormatoId) return 'Todos los formatos';
    const t = this.tipos.find(t => t.id === tipoFormatoId);
    return t ? t.nombre : '-';
  }

  agregar() {
    this.editando = false;
    this.editandoId = null;
    this.form = { categoria: this.filtroCategoria || '', tipoFormatoId: null, valor: '', orden: 0 };
    this.mostrarModal = true;
  }

  editar(o: any) {
    this.editando = true;
    this.editandoId = o.id;
    this.form = { categoria: o.categoria, tipoFormatoId: o.tipoFormatoId, valor: o.valor, orden: o.orden };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.categoria || !this.form.valor.trim()) { alert('Complete categoría y valor'); return; }

    const peticion = this.editando && this.editandoId
      ? this.service.editar(this.editandoId, this.form)
      : this.service.crear(this.form);

    peticion.subscribe({
      next: () => { this.cargar(); this.cerrarModal(); },
      error: (e) => { console.error(e); alert('Error guardando'); }
    });
  }

  cambiarEstado(o: any) {
    this.service.cambiarEstado(o.id).subscribe({ next: () => this.cargar() });
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar esta opción?')) return;
    this.service.eliminar(id).subscribe({ next: () => this.cargar() });
  }

  cerrarModal() { this.mostrarModal = false; }
}