import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import Swal from 'sweetalert2';

import { VehiculosService }   from '../../services/vehiculos.service';
import { ConductoresService } from '../../services/conductores.service';
import { PermisosService }    from '../../services/permisos.service';
import { Vehiculo }           from '../../models/vehiculo.model';
import { environment }        from '../../../environments/environment';

@Component({
  selector: 'app-vehiculos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vehiculos.html',
  styleUrls: ['./vehiculos.scss']
})
export class VehiculosComponent implements OnInit {

  vehiculos:            Vehiculo[] = [];
  conductores:          any[]      = [];
  mostrarModal        = false;
  vehiculoSeleccionado: any        = null;
  modoEdicion         = false;
  textoBusqueda       = '';
  paginaActual        = 1;
  itemsPorPagina      = 5;
  archivoSeleccionado: File | null = null;

  fotosUrl = environment.apiUrl.replace('/api', '') + '/vehiculos';

  nuevo: Vehiculo = {
    id: 0, placa: '', marca: '', modelo: '',
    modeloAnio: 2024, color: '', estado: 'Activo', conductorId: 0
  };

  // ── Permisos granulares ───────────────────────────────────────────────────────
  get puedeCrear():    boolean { return this.permisosService.puedeCrear('vehiculos'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('vehiculos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('vehiculos'); }

  constructor(
    private vehiculosService:   VehiculosService,
    private conductoresService: ConductoresService,
    private permisosService:    PermisosService
  ) {}

  ngOnInit(): void {
    this.obtenerVehiculos();
    this.obtenerConductores();
  }

  obtenerVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next:  (data) => this.vehiculos = data,
      error: (err)  => console.error(err)
    });
  }

  obtenerConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next:  (data) => this.conductores = data,
      error: (err)  => console.error(err)
    });
  }

  agregar() {
    this.modoEdicion = false;
    this.nuevo = {
      id: 0, placa: '', marca: '', modelo: '',
      modeloAnio: 2024, color: '', estado: 'Activo', conductorId: 0
    };
    this.archivoSeleccionado  = null;
    this.vehiculoSeleccionado = null;
    this.mostrarModal         = true;
  }

  editar(vehiculo: any) {
    this.modoEdicion = true;
    this.nuevo = {
      id:         vehiculo.id,
      placa:      vehiculo.placa,
      marca:      vehiculo.marca,
      modelo:     vehiculo.modelo,
      modeloAnio: vehiculo.año,
      color:      vehiculo.color,
      estado:     vehiculo.estado,
      conductorId: vehiculo.conductorId
    };
    this.mostrarModal = true;
  }

  cerrar() { this.mostrarModal = false; }

  seleccionarImagen(event: any) {
    if (event.target.files.length > 0)
      this.archivoSeleccionado = event.target.files[0];
  }

  guardar() {
    if (this.nuevo.conductorId === 0) {
      Swal.fire({ icon: 'warning', title: 'Seleccione un conductor' });
      return;
    }

    const formData = new FormData();
    formData.append('Placa',       this.nuevo.placa);
    formData.append('Marca',       this.nuevo.marca);
    formData.append('Modelo',      this.nuevo.modelo);
    formData.append('Año',         this.nuevo.modeloAnio.toString());
    formData.append('Color',       this.nuevo.color);
    formData.append('Estado',      this.nuevo.estado);
    formData.append('ConductorId', this.nuevo.conductorId.toString());
    if (this.archivoSeleccionado)
      formData.append('foto', this.archivoSeleccionado);

    const peticion = this.modoEdicion
      ? this.vehiculosService.editarVehiculo(this.nuevo.id, formData)
      : this.vehiculosService.crearVehiculo(formData);

    peticion.subscribe({
      next: () => {
        this.obtenerVehiculos();
        this.cerrar();
        Swal.fire({
          icon: 'success',
          title: this.modoEdicion ? 'Vehículo actualizado' : 'Vehículo guardado',
          timer: 1500,
          showConfirmButton: false
        });
      },
      error: (err) => {
        console.log(err);
        Swal.fire({ icon: 'error', title: 'Error al guardar' });
      }
    });
  }

  eliminar(id: number) {
    Swal.fire({
      title: '¿Eliminar vehículo?',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar'
    }).then((r) => {
      if (r.isConfirmed) {
        this.vehiculosService.eliminarVehiculo(id).subscribe({
          next: () => {
            this.obtenerVehiculos();
            Swal.fire({ icon: 'success', title: 'Eliminado', timer: 1200, showConfirmButton: false });
          }
        });
      }
    });
  }

  ver(vehiculo: any) { this.vehiculoSeleccionado = vehiculo; }

  get vehiculosFiltrados() {
    const q = this.textoBusqueda.toLowerCase();
    return this.vehiculos.filter(v =>
      v.placa?.toLowerCase().includes(q)  ||
      v.marca?.toLowerCase().includes(q)  ||
      v.modelo?.toLowerCase().includes(q) ||
      v.color?.toLowerCase().includes(q)
    );
  }
}