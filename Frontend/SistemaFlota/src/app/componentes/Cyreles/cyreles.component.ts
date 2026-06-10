import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CyrelesService } from '../../services/cyreles.service';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

type Vista = 'cajones' | 'registros' | 'detalle';
type Modal = 'crear-cajon' | 'editar-cajon' | 'crear-registro' | 'editar-registro' | null;
type Orden = 'nombre-asc' | 'nombre-desc' | 'fecha-asc' | 'fecha-desc';

@Component({
  selector: 'app-cyreles',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cyreles.component.html',
  styleUrls: ['./cyreles.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CyrelesComponent implements OnInit {

  vista: Vista = 'cajones';
  modal: Modal = null;

  cajones: any[] = [];
  cajonesFiltrados: any[] = [];
  registros: any[] = [];
  registrosFiltrados: any[] = [];
  cajonActual: any = null;
  registroActual: any = null;

  cargando = false;
  guardando = false;
  mensajeError = '';
  mensajeExito = '';

  // ── Filtros cajones ───────────────────────────────────────────────────────
  filtroCajonBusqueda = '';
  filtroCajonEstado = ''; // '' | 'con' | 'sin'

  // ── Filtros registros ─────────────────────────────────────────────────────
  filtroNombre = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';
  ordenRegistros: Orden = 'nombre-asc';

  // ── Formularios ───────────────────────────────────────────────────────────
  formCajon = { numero: 0, descripcion: '' };
  formRegistro = { nombre: '' };
  fotoFile: File | null = null;
  fotoPreview: string | null = null;

  constructor(private svc: CyrelesService, private cdr: ChangeDetectorRef) { }

  ngOnInit() { this.cargarCajones(); }

  // ── Cajones ────────────────────────────────────────────────────────────────
  cargarCajones() {
    this.cargando = true;
    this.svc.getCajones().subscribe({
      next: data => { this.cajones = data; this.aplicarFiltrosCajones(); this.cargando = false; this.cdr.markForCheck(); },
      error: () => { this.cargando = false; this.mostrarError('Error al cargar cajones'); this.cdr.markForCheck(); }
    });
  }

  aplicarFiltrosCajones() {
    let base = [...this.cajones];
    const q = this.filtroCajonBusqueda.toLowerCase();
    if (q) base = base.filter(c =>
      c.numero.toString().includes(q) ||
      (c.descripcion ?? '').toLowerCase().includes(q)
    );
    if (this.filtroCajonEstado === 'con') base = base.filter(c => c.totalRegistros > 0);
    if (this.filtroCajonEstado === 'sin') base = base.filter(c => c.totalRegistros === 0);
    this.cajonesFiltrados = base;
    this.cdr.markForCheck();
  }

  limpiarFiltrosCajones() {
    this.filtroCajonBusqueda = '';
    this.filtroCajonEstado = '';
    this.aplicarFiltrosCajones();
  }

  abrirCajon(cajon: any) {
    this.cajonActual = cajon;
    this.filtroNombre = '';
    this.filtroFechaDesde = '';
    this.filtroFechaHasta = '';
    this.ordenRegistros = 'nombre-asc';
    this.vista = 'registros';
    this.cargarRegistros(cajon.id);
  }

  volverACajones() {
    this.vista = 'cajones'; this.cajonActual = null; this.registroActual = null;
    this.cargarCajones();
  }

  abrirDetalle(registro: any) {
    this.registroActual = registro; this.vista = 'detalle'; this.cdr.markForCheck();
  }

  volverARegistros() {
    this.vista = 'registros'; this.registroActual = null; this.cdr.markForCheck();
  }

  // ── Registros ──────────────────────────────────────────────────────────────
  cargarRegistros(cajonId: number) {
    this.cargando = true;
    this.svc.getRegistros({ cajonId }).subscribe({
      next: data => {
        this.registros = data;
        this.aplicarFiltrosRegistros();
        this.cargando = false;
        this.cdr.markForCheck();
      },
      error: () => { this.cargando = false; this.mostrarError('Error al cargar registros'); this.cdr.markForCheck(); }
    });
  }

  aplicarFiltrosRegistros() {
    let base = [...this.registros];
    const q = this.filtroNombre.toLowerCase();
    if (q) base = base.filter(r => r.nombre.toLowerCase().includes(q));
    if (this.filtroFechaDesde) base = base.filter(r => new Date(r.fechaCreacion) >= new Date(this.filtroFechaDesde));
    if (this.filtroFechaHasta) base = base.filter(r => new Date(r.fechaCreacion) <= new Date(this.filtroFechaHasta + 'T23:59:59'));

    switch (this.ordenRegistros) {
      case 'nombre-asc': base.sort((a, b) => a.nombre.localeCompare(b.nombre)); break;
      case 'nombre-desc': base.sort((a, b) => b.nombre.localeCompare(a.nombre)); break;
      case 'fecha-asc': base.sort((a, b) => new Date(a.fechaCreacion).getTime() - new Date(b.fechaCreacion).getTime()); break;
      case 'fecha-desc': base.sort((a, b) => new Date(b.fechaCreacion).getTime() - new Date(a.fechaCreacion).getTime()); break;
    }
    this.registrosFiltrados = base;
    this.cdr.markForCheck();
  }

  limpiarFiltrosRegistros() {
    this.filtroNombre = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.ordenRegistros = 'nombre-asc';
    this.aplicarFiltrosRegistros();
  }

  get hayFiltrosRegistros(): boolean {
    return !!(this.filtroNombre || this.filtroFechaDesde || this.filtroFechaHasta || this.ordenRegistros !== 'nombre-asc');
  }

  get hayFiltrosCajones(): boolean {
    return !!(this.filtroCajonBusqueda || this.filtroCajonEstado);
  }

  // ── Modales ────────────────────────────────────────────────────────────────
  abrirCrearCajon() {
    this.formCajon = { numero: 0, descripcion: '' }; this.modal = 'crear-cajon'; this.cdr.markForCheck();
  }

  abrirEditarCajon(cajon: any, e: Event) {
    e.stopPropagation();
    this.cajonActual = cajon;
    this.formCajon = { numero: cajon.numero, descripcion: cajon.descripcion ?? '' };
    this.modal = 'editar-cajon'; this.cdr.markForCheck();
  }

  abrirCrearRegistro() {
    this.formRegistro = { nombre: '' }; this.fotoFile = null; this.fotoPreview = null;
    this.modal = 'crear-registro'; this.cdr.markForCheck();
  }

  abrirEditarRegistro(r: any) {
    this.registroActual = r;
    this.formRegistro = { nombre: r.nombre };
    this.fotoFile = null;
    this.fotoPreview = r.foto ? this.svc.getFotoUrl(r.foto) : null;
    this.modal = 'editar-registro'; this.cdr.markForCheck();
  }

  cerrarModal() {
    this.modal = null; this.fotoFile = null; this.fotoPreview = null; this.mensajeError = ''; this.cdr.markForCheck();
  }

  // ── Guardar ────────────────────────────────────────────────────────────────
  guardarCajon() {
    if (!this.formCajon.numero) { this.mostrarError('El número de cajón es obligatorio'); return; }
    this.guardando = true;
    const obs = this.modal === 'crear-cajon'
      ? this.svc.crearCajon({ numero: this.formCajon.numero, descripcion: this.formCajon.descripcion || undefined })
      : this.svc.editarCajon(this.cajonActual.id, { numero: this.formCajon.numero, descripcion: this.formCajon.descripcion || undefined });
    obs.subscribe({
      next: () => { this.guardando = false; this.cerrarModal(); this.mostrarExito(this.modal === 'crear-cajon' ? 'Cajón creado' : 'Cajón actualizado'); this.cargarCajones(); },
      error: (err: any) => { this.guardando = false; this.mostrarError(typeof err?.error === 'string' ? err.error : 'Error al guardar cajón'); this.cdr.markForCheck(); }
    });
  }

  guardarRegistro() {
    if (!this.formRegistro.nombre.trim()) { this.mostrarError('El nombre es obligatorio'); return; }
    this.guardando = true;
    const obs = this.modal === 'crear-registro'
      ? this.svc.crearRegistro(this.cajonActual.id, this.formRegistro.nombre, this.fotoFile ?? undefined)
      : this.svc.editarRegistro(this.registroActual.id, this.cajonActual.id, this.formRegistro.nombre, this.fotoFile ?? undefined);
    obs.subscribe({
      next: () => { this.guardando = false; this.cerrarModal(); this.mostrarExito(this.modal === 'crear-registro' ? 'Registro creado' : 'Registro actualizado'); this.cargarRegistros(this.cajonActual.id); },
      error: () => { this.guardando = false; this.mostrarError('Error al guardar'); this.cdr.markForCheck(); }
    });
  }

  // ── Eliminar ───────────────────────────────────────────────────────────────
  eliminarCajon(cajon: any, e: Event) {
    e.stopPropagation();
    if (!confirm(`¿Eliminar el Cajón ${cajon.numero} y todos sus registros?`)) return;
    this.svc.eliminarCajon(cajon.id).subscribe({
      next: () => { this.mostrarExito('Cajón eliminado'); this.cargarCajones(); },
      error: () => this.mostrarError('Error al eliminar')
    });
  }

  eliminarRegistro(r: any) {
    if (!confirm(`¿Eliminar "${r.nombre}"?`)) return;
    this.svc.eliminarRegistro(r.id).subscribe({
      next: () => { this.mostrarExito('Registro eliminado'); if (this.vista === 'detalle') this.volverARegistros(); this.cargarRegistros(this.cajonActual.id); },
      error: () => this.mostrarError('Error al eliminar')
    });
  }

  // ── Foto ───────────────────────────────────────────────────────────────────
  onFoto(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.fotoFile = file;
    const reader = new FileReader();
    reader.onload = e => { this.fotoPreview = e.target?.result as string; this.cdr.markForCheck(); };
    reader.readAsDataURL(file);
  }

  getFotoUrl(nombre: string) { return this.svc.getFotoUrl(nombre); }

  // ── Exportar ───────────────────────────────────────────────────────────────
  exportarExcel() {
    const datos = this.registrosFiltrados.map((r, i) => ({
      'N°': i + 1, 'Cajón': `Cajón ${r.cajonNumero}`,
      'Nombre': r.nombre, 'Fecha': new Date(r.fechaCreacion).toLocaleDateString('es-CO'),
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();
    hoja['!cols'] = [{ wch: 5 }, { wch: 12 }, { wch: 40 }, { wch: 14 }];
    XLSX.utils.book_append_sheet(libro, hoja, `Cajón ${this.cajonActual?.numero}`);
    XLSX.writeFile(libro, `cajon_${this.cajonActual?.numero}_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }

  exportarPDF() {
    const doc = new jsPDF();
    const VERDE = [21, 128, 61] as [number, number, number];
    doc.setFillColor(...VERDE); doc.rect(0, 0, 210, 20, 'F');
    doc.setFontSize(14); doc.setTextColor(255, 255, 255); doc.setFont('helvetica', 'bold');
    doc.text(`CYRELES — Cajón ${this.cajonActual?.numero}`, 105, 13, { align: 'center' });
    doc.setFontSize(8); doc.setTextColor(100, 100, 100); doc.setFont('helvetica', 'normal');
    doc.text(`Generado: ${new Date().toLocaleDateString('es-CO')} | Registros: ${this.registrosFiltrados.length}`, 105, 25, { align: 'center' });
    autoTable(doc, {
      startY: 30,
      head: [['N°', 'Nombre de Impresión', 'Fecha']],
      body: this.registrosFiltrados.map((r, i) => [i + 1, r.nombre, new Date(r.fechaCreacion).toLocaleDateString('es-CO')]),
      headStyles: { fillColor: VERDE, textColor: [255, 255, 255], fontStyle: 'bold', fontSize: 9 },
      bodyStyles: { fontSize: 9 }, alternateRowStyles: { fillColor: [245, 250, 245] },
      columnStyles: { 0: { cellWidth: 12 }, 1: { cellWidth: 140 }, 2: { cellWidth: 30 } },
    });
    doc.save(`cajon_${this.cajonActual?.numero}_${new Date().toISOString().slice(0, 10)}.pdf`);
  }

  // ── Helpers ────────────────────────────────────────────────────────────────
  private mostrarError(msg: string) {
    this.mensajeError = msg;
    setTimeout(() => { this.mensajeError = ''; this.cdr.markForCheck(); }, 4000);
    this.cdr.markForCheck();
  }

  private mostrarExito(msg: string) {
    this.mensajeExito = msg;
    setTimeout(() => { this.mensajeExito = ''; this.cdr.markForCheck(); }, 3000);
    this.cdr.markForCheck();
  }
}