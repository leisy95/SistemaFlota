import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../environments/environment';
import { PermisosService } from '../../../core/services/permisos.service';
import { CentroInformacionService } from '../../../core/services/centro-informacion.service';

@Component({
  selector: 'app-centro-informacion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './centro-informacion.html',
  styleUrls: ['./centro-informacion.scss']
})
export class CentroInformacionComponent implements OnInit {
  tabActual = 'rutas';
  rutas: any[] = [];
  emergencias: any[] = [];
  baseUrl = environment.fotosUrl;

  mostrarModalRuta = false;
  mostrarModalEmergencia = false;
  editandoEmergencia: any = null;

  formRuta = { nombre: '', descripcion: '' };
  archivoRuta: File | null = null;

  formEmergencia = { ciudad: '', tipo: '', numero: '', observaciones: '' };

  readonly tiposEmergencia = ['Policía', 'Bomberos', 'Ambulancia', 'Grúa', 'Hospital', 'Cruz Roja', 'Defensa Civil', 'Otro'];

  get puedeVer(): boolean { return this.permisosService.puedeVer('centro-informacion'); }
  get puedeCrear(): boolean { return this.permisosService.puedeCrear('centro-informacion'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('centro-informacion'); }

  constructor(
    private service: CentroInformacionService,
    private permisosService: PermisosService
  ) {}

  ngOnInit(): void { this.cargarDatos(); }

  cargarDatos() {
    this.service.getRutas().subscribe({ next: d => this.rutas = d, error: e => console.error(e) });
    this.service.getEmergencia().subscribe({ next: d => this.emergencias = d, error: e => console.error(e) });
  }

  seleccionarArchivo(event: any) { this.archivoRuta = event.target.files[0]; }

  guardarRuta() {
    if (!this.formRuta.nombre) { alert('Ingrese el nombre'); return; }
    const fd = new FormData();
    fd.append('Nombre', this.formRuta.nombre);
    if (this.formRuta.descripcion) fd.append('Descripcion', this.formRuta.descripcion);
    if (this.archivoRuta) fd.append('archivo', this.archivoRuta);
    this.service.crearRuta(fd).subscribe({
      next: () => { this.mostrarModalRuta = false; this.formRuta = { nombre: '', descripcion: '' }; this.archivoRuta = null; this.cargarDatos(); },
      error: e => { console.error(e); alert('Error guardando ruta'); }
    });
  }

  eliminarRuta(id: number) {
    if (!confirm('¿Eliminar esta ruta?')) return;
    this.service.eliminarRuta(id).subscribe({ next: () => this.cargarDatos() });
  }

  abrirModalEmergencia(e?: any) {
    if (e) { this.editandoEmergencia = e; this.formEmergencia = { ciudad: e.ciudad, tipo: e.tipo, numero: e.numero, observaciones: e.observaciones ?? '' }; }
    else { this.editandoEmergencia = null; this.formEmergencia = { ciudad: '', tipo: '', numero: '', observaciones: '' }; }
    this.mostrarModalEmergencia = true;
  }

  guardarEmergencia() {
    if (!this.formEmergencia.ciudad || !this.formEmergencia.tipo || !this.formEmergencia.numero) { alert('Complete los campos obligatorios'); return; }
    const obs = this.editandoEmergencia
      ? this.service.actualizarEmergencia(this.editandoEmergencia.id, this.formEmergencia)
      : this.service.crearEmergencia(this.formEmergencia);
    obs.subscribe({ next: () => { this.mostrarModalEmergencia = false; this.cargarDatos(); }, error: e => console.error(e) });
  }

  eliminarEmergencia(id: number) {
    if (!confirm('¿Eliminar este número?')) return;
    this.service.eliminarEmergencia(id).subscribe({ next: () => this.cargarDatos() });
  }

  get ciudades(): string[] { return [...new Set(this.emergencias.map(e => e.ciudad))]; }
  emergenciasPorCiudad(ciudad: string) { return this.emergencias.filter(e => e.ciudad === ciudad); }
}
