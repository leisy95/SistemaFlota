import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Conductor, ConductoresService } from '../../../core/servicios/conductores.service';
@Component({
  selector: 'app-conductor-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './conductor-form.html',
  styleUrl: './conductor-form.scss'
})
export class ConductorForm implements OnInit {

  conductores: Conductor[] = [];

  nuevo: Conductor = {
    nombre: '',
    licencia: '',
    telefono: '',
    email: ''
  };

  archivo!: File; 

  editando = false;
  filtro = '';

  constructor(private service: ConductoresService) {}

  ngOnInit() {
    this.cargarConductores();
  }

  cargarConductores() {
    this.service.obtenerConductores().subscribe({
      next: data => this.conductores = data,
      error: err => console.error(err)
    });
  }

  // CAPTURAR FOTO
  onFileSelected(event: any) {
    this.archivo = event.target.files[0];
  }

  guardar() {

    if (!this.nuevo.nombre || !this.nuevo.licencia) {
      alert('Campos obligatorios');
      return;
    }

    // EDITAR (sin foto por ahora)
    if (this.editando && this.nuevo.id) {
      this.service.actualizarConductor(this.nuevo).subscribe(() => {
        this.cargarConductores();
        this.reset();
      });
    } 
    // CREAR CON FOTO
    else {

      const formData = new FormData();

      formData.append('Nombre', this.nuevo.nombre);
      formData.append('Licencia', this.nuevo.licencia);
      formData.append('Telefono', this.nuevo.telefono);
      formData.append('Email', this.nuevo.email);

      if (this.archivo) {
        formData.append('foto', this.archivo);
      }

      this.service.crearConductor(formData).subscribe(() => {
        this.cargarConductores();
        this.reset();
      });
    }
  }

  editar(c: Conductor) {
    this.nuevo = { ...c };
    this.editando = true;
  }

  reset() {
    this.nuevo = {
      nombre: '',
      licencia: '',
      telefono: '',
      email: ''
    };
    this.editando = false;
  }

  eliminar(id?: number) {
    if (!id) return;

    this.service.eliminarConductor(id).subscribe(() => {
      this.cargarConductores();
    });
  }

  get conductoresFiltrados() {
    return this.conductores.filter(c =>
      c.nombre.toLowerCase().includes(this.filtro.toLowerCase())
    );
  }
}