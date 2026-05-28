import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentosService } from '../../services/documentos.service';
import { VehiculosService } from '../../services/vehiculos.service';

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documentos.html',
  styleUrls: ['./documentos.scss']
})

export class DocumentosComponent implements OnInit {

  // TABS
  tabActual: 'vehiculo' | 'generales' | 'vencer' = 'vehiculo';

  // VEHÍCULO
  vehiculos:          any[] = [];
  vehiculoSeleccionado = 0;
  documentosVehiculo:  any[] = [];

  // GENERALES
  documentosGenerales: any[] = [];
  filtroCategoria      = '';

  // POR VENCER
  porVencer: any = { vehiculo: [], generales: [], total: 0 };

  // MODAL
  mostrarModal  = false;
  tipoSubida:   'vehiculo' | 'general' = 'vehiculo';
  subiendo      = false;
  archivoSeleccionado: File | null = null;

  formVehiculo = {
    vehiculoId:       0,
    tipoDocumento:    '',
    nombre:           '',
    descripcion:      '',
    fechaVencimiento: ''
  };

  formGeneral = {
    nombre:           '',
    descripcion:      '',
    categoria:        'General',
    fechaVencimiento: ''
  };

  readonly tiposDocumentoVehiculo = [
    'SOAT',
    'Tecnomecanica',
    'TarjetaPropiedad',
    'PermisoOperacion',
    'PolizaResponsabilidad',
    'Otro'
  ];

  readonly categoriasGenerales = [
    'General',
    'Contratos',
    'Legal',
    'Seguros',
    'RRHH',
    'Financiero',
    'Operaciones',
    'Otro'
  ];

  constructor(
    private documentosService: DocumentosService,
    private vehiculosService:  VehiculosService
  ) {}

  ngOnInit(): void {
    this.cargarVehiculos();
    this.cargarDocumentosGenerales();
    this.cargarPorVencer();
  }

  // =========================
  // TABS
  // =========================

  cambiarTab(tab: 'vehiculo' | 'generales' | 'vencer') {
    this.tabActual = tab;
    if (tab === 'vencer') this.cargarPorVencer();
  }

  // =========================
  // VEHÍCULOS
  // =========================

  cargarVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err) => console.error(err)
    });
  }

  seleccionarVehiculo() {
    if (this.vehiculoSeleccionado) {
      this.cargarDocumentosVehiculo();
    }
  }

  cargarDocumentosVehiculo() {
    this.documentosService
      .obtenerDocumentosVehiculo(this.vehiculoSeleccionado)
      .subscribe({
        next: (data) => this.documentosVehiculo = data,
        error: (err) => console.error(err)
      });
  }

  // =========================
  // GENERALES
  // =========================

  cargarDocumentosGenerales() {
    this.documentosService
      .obtenerDocumentosGenerales(this.filtroCategoria || undefined)
      .subscribe({
        next: (data) => this.documentosGenerales = data,
        error: (err) => console.error(err)
      });
  }

  // =========================
  // POR VENCER
  // =========================

  cargarPorVencer() {
    this.documentosService.obtenerPorVencer().subscribe({
      next: (data) => this.porVencer = data,
      error: (err) => console.error(err)
    });
  }

  // =========================
  // SUBIR DOCUMENTO
  // =========================

  abrirSubir(tipo: 'vehiculo' | 'general') {
    this.tipoSubida          = tipo;
    this.archivoSeleccionado = null;
    this.formVehiculo = {
      vehiculoId: this.vehiculoSeleccionado,
      tipoDocumento: '', nombre: '',
      descripcion: '', fechaVencimiento: ''
    };
    this.formGeneral = {
      nombre: '', descripcion: '',
      categoria: 'General', fechaVencimiento: ''
    };
    this.mostrarModal = true;
  }

  seleccionarArchivo(event: any) {
    this.archivoSeleccionado = event.target.files[0] || null;
    if (this.archivoSeleccionado && !this.formVehiculo.nombre && !this.formGeneral.nombre) {
      const nombre = this.archivoSeleccionado.name.replace(/\.[^/.]+$/, '');
      if (this.tipoSubida === 'vehiculo') this.formVehiculo.nombre = nombre;
      else this.formGeneral.nombre = nombre;
    }
  }

  subirDocumento() {
    if (!this.archivoSeleccionado) { alert('Seleccione un archivo'); return; }

    this.subiendo = true;
    const formData = new FormData();

    if (this.tipoSubida === 'vehiculo') {
      if (!this.formVehiculo.vehiculoId)    { alert('Seleccione un vehículo'); this.subiendo = false; return; }
      if (!this.formVehiculo.tipoDocumento) { alert('Seleccione el tipo'); this.subiendo = false; return; }
      if (!this.formVehiculo.nombre)        { alert('Ingrese un nombre'); this.subiendo = false; return; }

      formData.append('VehiculoId',       this.formVehiculo.vehiculoId.toString());
      formData.append('TipoDocumento',    this.formVehiculo.tipoDocumento);
      formData.append('Nombre',           this.formVehiculo.nombre);
      formData.append('Descripcion',      this.formVehiculo.descripcion);
      formData.append('Archivo',          this.archivoSeleccionado);
      if (this.formVehiculo.fechaVencimiento)
        formData.append('FechaVencimiento', this.formVehiculo.fechaVencimiento);

      this.documentosService.subirDocumentoVehiculo(formData).subscribe({
        next: () => {
          this.subiendo = false;
          this.mostrarModal = false;
          this.cargarDocumentosVehiculo();
        },
        error: (err) => { console.error(err); this.subiendo = false; }
      });

    } else {
      if (!this.formGeneral.nombre) { alert('Ingrese un nombre'); this.subiendo = false; return; }

      formData.append('Nombre',      this.formGeneral.nombre);
      formData.append('Descripcion', this.formGeneral.descripcion);
      formData.append('Categoria',   this.formGeneral.categoria);
      formData.append('Archivo',     this.archivoSeleccionado);
      if (this.formGeneral.fechaVencimiento)
        formData.append('FechaVencimiento', this.formGeneral.fechaVencimiento);

      this.documentosService.subirDocumentoGeneral(formData).subscribe({
        next: () => {
          this.subiendo = false;
          this.mostrarModal = false;
          this.cargarDocumentosGenerales();
        },
        error: (err) => { console.error(err); this.subiendo = false; }
      });
    }
  }

  // =========================
  // ELIMINAR
  // =========================

  eliminarVehiculo(id: number) {
    if (!confirm('¿Eliminar documento?')) return;
    this.documentosService.eliminarDocumentoVehiculo(id).subscribe({
      next: () => this.cargarDocumentosVehiculo(),
      error: (err) => console.error(err)
    });
  }

  eliminarGeneral(id: number) {
    if (!confirm('¿Eliminar documento?')) return;
    this.documentosService.eliminarDocumentoGeneral(id).subscribe({
      next: () => this.cargarDocumentosGenerales(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // UTILIDADES
  // =========================

  getIconoExtension(ext: string): string {
    if (!ext) return '📄';
    const e = ext.toLowerCase();
    if (e === '.pdf')                         return '📕';
    if (['.jpg','.jpeg','.png'].includes(e))  return '🖼️';
    if (['.doc','.docx'].includes(e))         return '📝';
    if (['.xls','.xlsx'].includes(e))         return '📊';
    return '📄';
  }

  getTamano(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024)        return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  estaVencido(fecha: string): boolean {
    if (!fecha) return false;
    return new Date(fecha) < new Date();
  }

  diasParaVencer(fecha: string): number {
    if (!fecha) return 999;
    const diff = new Date(fecha).getTime() - new Date().getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  getUrlDocumento(tipo: string, archivo: string): string {
    const base = 'https://localhost:7293/documentos';
    return tipo === 'vehiculo'
      ? `${base}/vehiculos/${archivo}`
      : `${base}/generales/${archivo}`;
  }

}