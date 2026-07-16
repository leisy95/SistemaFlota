import { Component, OnInit } from '@angular/core';
import { CommonModule }      from '@angular/common';
import { FormsModule }       from '@angular/forms';
import { HttpClientModule }  from '@angular/common/http';
import Swal from 'sweetalert2';

import { ConductoresService } from '../../../core/services/conductores.service';
import { PermisosService }    from '../../../core/services/permisos.service';
import { environment }        from '../../../../environments/environment';
import { Conductor }          from '../../../core/models/conductor.model';
import { UsuariosService }    from '../../../core/services/usuarios.service';
import { HojaVidaComponent } from '../../configuracion/hoja-vida/hoja-vida.component';

@Component({
  selector: 'app-conductores',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule, HojaVidaComponent],
  templateUrl: './conductores.component.html',
  styleUrls: ['./conductores.component.scss']
})
export class ConductoresComponent implements OnInit {

  conductores:          Conductor[] = [];
  conductoresFiltrados: Conductor[] = [];
  conductoresPaginados: Conductor[] = [];

  busqueda       = '';
  paginaActual   = 1;
  itemsPorPagina = 5;
  mostrarModal   = false;
  editando       = false;
  archivoSeleccionado:   File | null      = null;
  conductorSeleccionado: Conductor | null = null;

  // â”€â”€ Hoja de vida â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  mostrarHojaVida    = false;
  conductorHojaVida: Conductor | null = null;

  nuevo: Conductor = { id: 0, nombre: '', licencia: '', telefono: '', email: '', foto: '' };

  apiUrl   = `${environment.apiUrl}/Conductores`;
  fotosUrl = environment.fotosUrl + '/fotos';
  mostrarModalPassword = true;
  passwordAcceso = '';
  errorPassword = '';
  accesoVerificado = false;

  get puedeCrear():    boolean { return this.permisosService.puedeCrear('conductores'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('conductores'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('conductores'); }

  constructor(
    private conductoresService: ConductoresService,
    private usuariosService: UsuariosService,
    private permisosService:    PermisosService
  ) {}

  ngOnInit(): void { this.obtenerConductores(); }

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
        this.actualizarPaginado();
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
      c.nombre.toLowerCase().includes(texto)   ||
      c.licencia.toLowerCase().includes(texto) ||
      c.email.toLowerCase().includes(texto)
    );
    this.paginaActual = 1;
    this.actualizarPaginado();
  }

  private actualizarPaginado() {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    this.conductoresPaginados = this.conductoresFiltrados.slice(inicio, inicio + this.itemsPorPagina);
  }

  paginaSiguiente() {
    const total = Math.ceil(this.conductoresFiltrados.length / this.itemsPorPagina);
    if (this.paginaActual < total) { this.paginaActual++; this.actualizarPaginado(); }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) { this.paginaActual--; this.actualizarPaginado(); }
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
    const fd = new FormData();
    fd.append('nombre',   this.nuevo.nombre);
    fd.append('licencia', this.nuevo.licencia);
    fd.append('telefono', this.nuevo.telefono);
    fd.append('email',    this.nuevo.email);
    if (this.archivoSeleccionado)
      fd.append('foto', this.archivoSeleccionado, this.archivoSeleccionado.name);

    const peticion = this.editando
      ? this.conductoresService.editarConductor(this.nuevo.id, fd)
      : this.conductoresService.crearConductor(fd);

    peticion.subscribe({
      next: () => {
        this.obtenerConductores(); this.cerrar();
        Swal.fire({ icon: 'success', title: this.editando ? 'Conductor actualizado' : 'Conductor guardado', showConfirmButton: false, timer: 1500 });
      },
      error: (err) => {
        console.error(err);
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo guardar' });
      }
    });
  }

  editar(conductor: Conductor) {
    this.editando = true;
    this.nuevo    = { ...conductor };
    this.mostrarModal = true;
  }

  eliminar(id: number) {
    Swal.fire({
      title: 'Â¿Eliminar conductor?', text: 'Esta acciÃ³n no se puede deshacer',
      icon: 'warning', showCancelButton: true,
      confirmButtonText: 'SÃ­, eliminar', cancelButtonText: 'Cancelar'
    }).then((r: any) => {
      if (r.isConfirmed) {
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
    this.mostrarHojaVida = false; // resetear si estaba abierta
  }

  // â”€â”€ Hoja de vida â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  abrirHojaVida(conductor: Conductor) {
    this.conductorHojaVida     = conductor;
    this.mostrarHojaVida       = true;
    this.conductorSeleccionado = null; // cerrar modal perfil
  }

  cerrarHojaVida() {
    this.mostrarHojaVida   = false;
    this.conductorHojaVida = null;
  }

  verificarAcceso() {
    const user = JSON.parse(sessionStorage.getItem('user') || '{}');
    this.usuariosService.verificarModulo(user.username, this.passwordAcceso).subscribe({
      next: () => {
        this.accesoVerificado = true;
        this.mostrarModalPassword = false;
        this.errorPassword = '';
      },
      error: (err: any) => {
        this.errorPassword = err.error?.error ?? 'Contrasena incorrecta';
      }
    });
  }

  cancelarAcceso() {
    window.history.back();
  }
}
