import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentosService } from '../../services/documentos.service';
import { VehiculosService }  from '../../services/vehiculos.service';
import { PermisosService }   from '../../services/permisos.service';

@Component({
  selector: 'app-documentos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documentos.html',
  styleUrls: ['./documentos.scss']
})
export class DocumentosComponent implements OnInit {

  tabActual: 'vehiculo' | 'generales' | 'vencer' = 'vehiculo';

  vehiculos:           any[] = [];
  vehiculoSeleccionado = 0;
  documentosVehiculo:  any[] = [];
  documentosVehiculoFiltrados: any[] = [];

  documentosGenerales: any[] = [];
  documentosGeneralesFiltrados: any[] = [];

  porVencer:    any   = { vehiculo: [], generales: [], total: 0 };
  porVencerFiltrados: any[] = [];

  filtroCategoria    = '';
  filtroTipoDoc      = '';
  filtroEstadoVeh    = '';
  filtroEstadoGen    = '';
  filtroDiasVencer   = '30';
  filtroBusqueda     = '';

  mostrarModal = false;
  tipoSubida:  'vehiculo' | 'general' = 'vehiculo';
  subiendo     = false;
  archivoSeleccionado: File | null = null;

  formVehiculo = { vehiculoId: 0, tipoDocumento: '', nombre: '', descripcion: '', fechaVencimiento: '' };
  formGeneral  = { nombre: '', descripcion: '', categoria: 'General', fechaVencimiento: '' };

  readonly tiposDocumentoVehiculo = [
    'SOAT','Tecnomecanica','TarjetaPropiedad',
    'PermisoOperacion','PolizaResponsabilidad','Otro'
  ];

  readonly categoriasGenerales = [
    'General','Contratos','Legal','Seguros',
    'RRHH','Financiero','Operaciones','Otro'
  ];

  // â”€â”€ Permisos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  get puedeCrear():    boolean { return this.permisosService.puedeCrear('documentos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('documentos'); }

  // â”€â”€ Stats rÃ¡pidos â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  get totalVehiculo():  number { return this.documentosVehiculo.length; }
  get vencidosVeh():    number { return this.documentosVehiculo.filter(d => this.estaVencido(d.fechaVencimiento)).length; }
  get porVencerVeh():   number { return this.documentosVehiculo.filter(d => !this.estaVencido(d.fechaVencimiento) && this.diasParaVencer(d.fechaVencimiento) <= 30).length; }
  get totalGenerales(): number { return this.documentosGenerales.length; }
  get vencidosGen():    number { return this.documentosGenerales.filter(d => this.estaVencido(d.fechaVencimiento)).length; }

  constructor(
    private documentosService: DocumentosService,
    private vehiculosService:  VehiculosService,
    private permisosService:   PermisosService
  ) {}

  ngOnInit(): void {
    this.cargarVehiculos();
    this.cargarDocumentosGenerales();
    this.cargarPorVencer();
  }

  cambiarTab(tab: 'vehiculo' | 'generales' | 'vencer') {
    this.tabActual = tab;
    if (tab === 'vencer') this.cargarPorVencer();
  }

  cargarVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err)  => console.error(err)
    });
  }

  seleccionarVehiculo() {
    if (this.vehiculoSeleccionado) this.cargarDocumentosVehiculo();
    else { this.documentosVehiculo = []; this.documentosVehiculoFiltrados = []; }
  }

  cargarDocumentosVehiculo() {
    this.documentosService.obtenerDocumentosVehiculo(this.vehiculoSeleccionado).subscribe({
      next: (data) => { this.documentosVehiculo = data; this.aplicarFiltrosVehiculo(); },
      error: (err)  => console.error(err)
    });
  }

  cargarDocumentosGenerales() {
    this.documentosService.obtenerDocumentosGenerales(this.filtroCategoria || undefined).subscribe({
      next: (data) => { this.documentosGenerales = data; this.aplicarFiltrosGenerales(); },
      error: (err)  => console.error(err)
    });
  }

  cargarPorVencer() {
    this.documentosService.obtenerPorVencer().subscribe({
      next: (data) => { this.porVencer = data; this.aplicarFiltroVencer(); },
      error: (err)  => console.error(err)
    });
  }

  // â”€â”€ Filtros vehÃ­culo â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  aplicarFiltrosVehiculo() {
    const q = this.filtroBusqueda.toLowerCase();
    this.documentosVehiculoFiltrados = this.documentosVehiculo.filter(d => {
      const okTipo   = !this.filtroTipoDoc   || d.tipoDocumento === this.filtroTipoDoc;
      const okEstado = !this.filtroEstadoVeh || this.getEstadoDoc(d) === this.filtroEstadoVeh;
      const okBusq   = !q || d.nombre?.toLowerCase().includes(q) || d.tipoDocumento?.toLowerCase().includes(q);
      return okTipo && okEstado && okBusq;
    });
  }

  limpiarFiltrosVehiculo() {
    this.filtroTipoDoc = ''; this.filtroEstadoVeh = ''; this.filtroBusqueda = '';
    this.aplicarFiltrosVehiculo();
  }

  // â”€â”€ Filtros generales â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  aplicarFiltrosGenerales() {
    const q = this.filtroBusqueda.toLowerCase();
    this.documentosGeneralesFiltrados = this.documentosGenerales.filter(d => {
      const okCat    = !this.filtroCategoria  || d.categoria === this.filtroCategoria;
      const okEstado = !this.filtroEstadoGen  || this.getEstadoDoc(d) === this.filtroEstadoGen;
      const okBusq   = !q || d.nombre?.toLowerCase().includes(q) || d.descripcion?.toLowerCase().includes(q);
      return okCat && okEstado && okBusq;
    });
  }

  limpiarFiltrosGenerales() {
    this.filtroCategoria = ''; this.filtroEstadoGen = ''; this.filtroBusqueda = '';
    this.aplicarFiltrosGenerales();
  }

  // â”€â”€ Filtro por vencer â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  aplicarFiltroVencer() {
    const dias = Number(this.filtroDiasVencer);
    const todos = [
      ...(this.porVencer.vehiculo  || []).map((d: any) => ({ ...d, _tipo: 'VehÃ­culo' })),
      ...(this.porVencer.generales || []).map((d: any) => ({ ...d, _tipo: 'General' }))
    ];
    this.porVencerFiltrados = todos.filter(d =>
      this.estaVencido(d.fechaVencimiento) || this.diasParaVencer(d.fechaVencimiento) <= dias
    );
  }

  // â”€â”€ Utilidad estado â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  getEstadoDoc(d: any): string {
    if (!d.fechaVencimiento) return 'Vigente';
    if (this.estaVencido(d.fechaVencimiento)) return 'Vencido';
    if (this.diasParaVencer(d.fechaVencimiento) <= 30) return 'PorVencer';
    return 'Vigente';
  }

  // â”€â”€ Modal subir â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
  abrirSubir(tipo: 'vehiculo' | 'general') {
    this.tipoSubida = tipo; this.archivoSeleccionado = null;
    this.formVehiculo = { vehiculoId: this.vehiculoSeleccionado, tipoDocumento: '', nombre: '', descripcion: '', fechaVencimiento: '' };
    this.formGeneral  = { nombre: '', descripcion: '', categoria: 'General', fechaVencimiento: '' };
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
    const fd = new FormData();

    if (this.tipoSubida === 'vehiculo') {
      if (!this.formVehiculo.vehiculoId)    { alert('Seleccione un vehÃ­culo'); this.subiendo = false; return; }
      if (!this.formVehiculo.tipoDocumento) { alert('Seleccione el tipo'); this.subiendo = false; return; }
      if (!this.formVehiculo.nombre)        { alert('Ingrese un nombre'); this.subiendo = false; return; }
      fd.append('VehiculoId',    this.formVehiculo.vehiculoId.toString());
      fd.append('TipoDocumento', this.formVehiculo.tipoDocumento);
      fd.append('Nombre',        this.formVehiculo.nombre);
      fd.append('Descripcion',   this.formVehiculo.descripcion);
      fd.append('Archivo',       this.archivoSeleccionado);
      if (this.formVehiculo.fechaVencimiento)
        fd.append('FechaVencimiento', this.formVehiculo.fechaVencimiento);
      this.documentosService.subirDocumentoVehiculo(fd).subscribe({
        next: () => { this.subiendo = false; this.mostrarModal = false; this.cargarDocumentosVehiculo(); },
        error: (err) => { console.error(err); this.subiendo = false; }
      });
    } else {
      if (!this.formGeneral.nombre) { alert('Ingrese un nombre'); this.subiendo = false; return; }
      fd.append('Nombre',      this.formGeneral.nombre);
      fd.append('Descripcion', this.formGeneral.descripcion);
      fd.append('Categoria',   this.formGeneral.categoria);
      fd.append('Archivo',     this.archivoSeleccionado);
      if (this.formGeneral.fechaVencimiento)
        fd.append('FechaVencimiento', this.formGeneral.fechaVencimiento);
      this.documentosService.subirDocumentoGeneral(fd).subscribe({
        next: () => { this.subiendo = false; this.mostrarModal = false; this.cargarDocumentosGenerales(); },
        error: (err) => { console.error(err); this.subiendo = false; }
      });
    }
  }

  eliminarVehiculo(id: number) {
    if (!confirm('Â¿Eliminar documento?')) return;
    this.documentosService.eliminarDocumentoVehiculo(id).subscribe({
      next: () => this.cargarDocumentosVehiculo(),
      error: (err) => console.error(err)
    });
  }

  eliminarGeneral(id: number) {
    if (!confirm('Â¿Eliminar documento?')) return;
    this.documentosService.eliminarDocumentoGeneral(id).subscribe({
      next: () => this.cargarDocumentosGenerales(),
      error: (err) => console.error(err)
    });
  }

  getIconoExtension(ext: string): string {
    if (!ext) return 'ðŸ“„';
    const e = ext.toLowerCase();
    if (e === '.pdf')                        return 'ðŸ“•';
    if (['.jpg','.jpeg','.png'].includes(e)) return 'ðŸ–¼ï¸';
    if (['.doc','.docx'].includes(e))        return 'ðŸ“';
    if (['.xls','.xlsx'].includes(e))        return 'ðŸ“Š';
    return 'ðŸ“„';
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
    return Math.ceil((new Date(fecha).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24));
  }

  getUrlDocumento(tipo: string, archivo: string): string {
    const base = 'https://api.gecobagsci.com/documentos';
    return tipo === 'vehiculo' ? `${base}/vehiculos/${archivo}` : `${base}/generales/${archivo}`;
  }
}
