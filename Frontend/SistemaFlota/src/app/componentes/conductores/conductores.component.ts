import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import Swal from 'sweetalert2';

import { ConductoresService } from '../../services/conductores.service';
import { PermisosService } from '../../services/permisos.service'; // ← NUEVO
import { environment } from '../../../environments/environment';
import { Conductor } from '../../models/conductor.model';

@Component({
  selector: 'app-conductores',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule
  ],
  templateUrl: './conductores.component.html',
  styleUrls: ['./conductores.component.scss']
})
export class ConductoresComponent implements OnInit {

  conductores: Conductor[] = [];
  conductoresFiltrados: Conductor[] = [];
  busqueda = '';
  paginaActual = 1;
  itemsPorPagina = 5;
  mostrarModal = false;
  editando = false;
  archivoSeleccionado: File | null = null;
  conductorSeleccionado: Conductor | null = null;

  nuevo: Conductor = {
    id: 0, nombre: '', licencia: '',
    telefono: '', email: '', foto: ''
  };

  apiUrl  = `${environment.apiUrl}/Conductores`;
  fotosUrl = environment.fotosUrl;

  constructor(
    private conductoresService: ConductoresService,
    private permisosService:    PermisosService  // ← NUEVO
  ) {}

  // ← NUEVOS GETTERS DE PERMISOS
  get puedeCrear():    boolean { return this.permisosService.puedeCrear('conductores'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('conductores'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('conductores'); }

  ngOnInit(): void {
    this.obtenerConductores();
  }

  obtenerConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next: (data) => {
        this.conductores = data.map(c => ({
          id:       c.id       ?? c.Id,
          nombre:   c.nombre   ?? c.Nombre,
          licencia: c.licencia ?? c.Licencia,
          telefono: c.telefono ?? c.Telefono,
          email:    c.email    ?? c.Email,
          foto:     (c.foto ?? c.Foto)
            ?.replace('wwwroot/fotos/', '')
            ?.replace('/fotos/', '')
        }));
        this.conductoresFiltrados = this.conductores;
      },
      error: (err) => {
        console.error(err);
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudieron cargar los conductores' });
      }
    });
  }

  filtrarConductores() {
    const texto = this.busqueda.toLowerCase();
    this.conductoresFiltrados = this.conductores.filter(c =>
      c.nombre.toLowerCase().includes(texto) ||
      c.licencia.toLowerCase().includes(texto) ||
      c.email.toLowerCase().includes(texto)
    );
    this.paginaActual = 1;
  }

  obtenerConductoresPaginados() {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    return this.conductoresFiltrados.slice(inicio, inicio + this.itemsPorPagina);
  }

  paginaSiguiente() {
    const totalPaginas = Math.ceil(this.conductoresFiltrados.length / this.itemsPorPagina);
    if (this.paginaActual < totalPaginas) this.paginaActual++;
  }

  paginaAnterior() {
    if (this.paginaActual > 1) this.paginaActual--;
  }

  agregar() {
    this.editando = false;
    this.nuevo    = { id: 0, nombre: '', licencia: '', telefono: '', email: '', foto: '' };
    this.archivoSeleccionado = null;
    this.mostrarModal = true;
  }

  cerrar() { this.mostrarModal = false; }

  seleccionarImagen(event: any) {
    if (event.target.files.length > 0)
      this.archivoSeleccionado = event.target.files[0];
  }

  guardar() {
    const formData = new FormData();
    formData.append('nombre',   this.nuevo.nombre);
    formData.append('licencia', this.nuevo.licencia);
    formData.append('telefono', this.nuevo.telefono);
    formData.append('email',    this.nuevo.email);

    if (this.archivoSeleccionado)
      formData.append('foto', this.archivoSeleccionado, this.archivoSeleccionado.name);

    if (!this.editando) {
      this.conductoresService.crearConductor(formData).subscribe({
        next: () => {
          this.obtenerConductores(); this.cerrar();
          Swal.fire({ icon: 'success', title: 'Conductor guardado', showConfirmButton: false, timer: 1500 });
        },
        error: (err) => {
          console.error(err);
          Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo guardar' });
        }
      });
    } else {
      this.conductoresService.editarConductor(this.nuevo.id, formData).subscribe({
        next: () => {
          this.obtenerConductores(); this.cerrar();
          Swal.fire({ icon: 'success', title: 'Conductor actualizado', showConfirmButton: false, timer: 1500 });
        },
        error: (err) => {
          console.error(err);
          Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo editar' });
        }
      });
    }
  }

  editar(conductor: Conductor) {
    this.editando = true;
    this.nuevo    = { ...conductor };
    this.mostrarModal = true;
  }

  eliminar(id: number) {
    Swal.fire({
      title: '¿Eliminar conductor?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    }).then((resultado: any) => {
      if (resultado.isConfirmed) {
        this.conductoresService.eliminarConductor(id).subscribe({
          next: () => {
            this.obtenerConductores();
            Swal.fire({ icon: 'success', title: 'Eliminado', showConfirmButton: false, timer: 1500 });
          },
          error: () => Swal.fire({ icon: 'error', title: 'Error al eliminar' })
        });
      }
    });
  }

  ver(conductor: Conductor) {
    this.conductorSeleccionado = { ...conductor };
  }
}