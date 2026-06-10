import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SeguimientosRrhhService } from '../../services/seguimientos-rrhh.service';
import {
  SeguimientoRrhh, SeguimientoRrhhFoto,
  MESES_RRHH, PRIORIDADES_RRHH, ESTADOS_RRHH, FUENTES_RRHH
} from '../../models/seguimientos-rrhh.model';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

type VistaModal = 'crear' | 'editar' | 'ver' | 'galeria' | null;

@Component({
  selector: 'app-seguimientos-rrhh',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './seguimientos-rrhh.component.html',
  styleUrls: ['./seguimientos-rrhh.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SeguimientosRrhhComponent implements OnInit {

  registros: SeguimientoRrhh[] = [];
  registrosFiltrados: SeguimientoRrhh[] = [];
  registroSeleccionado: SeguimientoRrhh | null = null;

  readonly meses       = MESES_RRHH;
  readonly prioridades = PRIORIDADES_RRHH;
  readonly estados     = ESTADOS_RRHH;
  readonly fuentes     = FUENTES_RRHH;
  readonly anioActual  = new Date().getFullYear();
  readonly anios       = Array.from({ length: 6 }, (_, i) => this.anioActual - 2 + i);

  cargando  = false;
  guardando = false;
  vistaModal: VistaModal = null;
  mensajeError = '';
  mensajeExito = '';

  fotosGaleria: SeguimientoRrhhFoto[] = [];
  fotoGaleriaActual = 0;

  filtros = { area: '', estado: '', prioridad: '', mes: 0, anio: 0 };

  form = {
    area: '', mes: new Date().getMonth() + 1, anio: new Date().getFullYear(),
    fuente: '', areas: '', descripcion: '', planAccionSugerido: '',
    factorRiesgo: '', prioridad: 'Media', responsable: '',
    fechaEjecucion: '', fechaSeguimiento: '', estado: 'Abierta', observaciones: '',
  };

  fotosEvidenciaFiles: File[]   = [];
  fotosSeguimientoFiles: File[] = [];
  previewsEvidencia: string[]   = [];
  previewsSeguimiento: string[] = [];

  constructor(
    private svc: SeguimientosRrhhService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() { this.cargar(); }

  // ── Carga ──────────────────────────────────────────────────────────────────
  cargar() {
    this.cargando = true;
    this.svc.getAll().subscribe({
      next: data => {
        this.registros = data;
        this.aplicarFiltros();
        this.cargando = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.cargando = false;
        this.mostrarError('Error al cargar seguimientos');
        this.cdr.markForCheck();
      }
    });
  }

  // ── Filtros ────────────────────────────────────────────────────────────────
  aplicarFiltros() {
    this.registrosFiltrados = this.registros.filter(r => {
      const okArea      = !this.filtros.area      || r.area.toLowerCase().includes(this.filtros.area.toLowerCase());
      const okEstado    = !this.filtros.estado    || r.estado === this.filtros.estado;
      const okPrioridad = !this.filtros.prioridad || r.prioridad === this.filtros.prioridad;
      const okMes       = !this.filtros.mes       || r.mes === +this.filtros.mes;
      const okAnio      = !this.filtros.anio      || r.anio === +this.filtros.anio;
      return okArea && okEstado && okPrioridad && okMes && okAnio;
    });
    this.cdr.markForCheck();
  }

  limpiarFiltros() {
    this.filtros = { area: '', estado: '', prioridad: '', mes: 0, anio: 0 };
    this.aplicarFiltros();
  }

  // ── Exportar Excel ─────────────────────────────────────────────────────────
  exportarExcel() {
    const datos = this.registrosFiltrados.map(r => ({
      'ID':                 r.id,
      'Área':               r.area,
      'Mes':                this.getNombreMes(r.mes),
      'Año':                r.anio,
      'Fuente':             r.fuente,
      'Áreas relacionadas': r.areas ?? '-',
      'Descripción':        r.descripcion,
      'Plan de Acción':     r.planAccionSugerido ?? '-',
      'Factor de Riesgo':   r.factorRiesgo ?? '-',
      'Prioridad':          r.prioridad,
      'Responsable':        r.responsable ?? '-',
      'Fecha Ejecución':    r.fechaEjecucion   ? new Date(r.fechaEjecucion).toLocaleDateString('es-CO')   : '-',
      'Fecha Seguimiento':  r.fechaSeguimiento ? new Date(r.fechaSeguimiento).toLocaleDateString('es-CO') : '-',
      'Estado':             r.estado,
      'Observaciones':      r.observaciones ?? '-',
      'Creado por':         r.nombreCreadoPor ?? r.creadoPor,
      'Fecha creación':     new Date(r.fechaCreacion).toLocaleDateString('es-CO'),
    }));

    const hoja  = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Seguimientos RRHH');

    hoja['!cols'] = [
      { wch: 5 }, { wch: 18 }, { wch: 12 }, { wch: 6 }, { wch: 28 },
      { wch: 20 }, { wch: 40 }, { wch: 35 }, { wch: 20 }, { wch: 10 },
      { wch: 20 }, { wch: 16 }, { wch: 16 }, { wch: 12 }, { wch: 30 },
      { wch: 18 }, { wch: 14 },
    ];

    XLSX.writeFile(libro, `seguimientos_rrhh_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }

  // ── Exportar PDF ───────────────────────────────────────────────────────────
  exportarPDF() {
    const doc   = new jsPDF('landscape');
    const VERDE = [21, 128, 61] as [number, number, number];

    doc.setFillColor(...VERDE);
    doc.rect(0, 0, 297, 20, 'F');
    doc.setFontSize(14);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text('SEGUIMIENTOS RRHH', 148, 13, { align: 'center' });

    doc.setFontSize(8);
    doc.setTextColor(100, 100, 100);
    doc.setFont('helvetica', 'normal');
    const fechaGen = new Date().toLocaleDateString('es-CO', { day: '2-digit', month: '2-digit', year: 'numeric' });
    doc.text(`Generado: ${fechaGen} | Total registros: ${this.registrosFiltrados.length}`, 148, 26, { align: 'center' });

    autoTable(doc, {
      startY: 30,
      head: [['#', 'Área', 'Mes/Año', 'Fuente', 'Descripción', 'Factor Riesgo', 'Prioridad', 'Responsable', 'F. Ejecución', 'Estado']],
      body: this.registrosFiltrados.map(r => [
        r.id,
        r.area,
        `${this.getNombreMes(r.mes)} ${r.anio}`,
        r.fuente,
        r.descripcion.length > 50 ? r.descripcion.substring(0, 50) + '...' : r.descripcion,
        r.factorRiesgo ?? '-',
        r.prioridad,
        r.responsable ?? '-',
        r.fechaEjecucion ? new Date(r.fechaEjecucion).toLocaleDateString('es-CO') : '-',
        r.estado,
      ]),
      headStyles: { fillColor: VERDE, textColor: [255, 255, 255], fontStyle: 'bold', fontSize: 7 },
      bodyStyles: { fontSize: 7 },
      alternateRowStyles: { fillColor: [245, 250, 245] },
      columnStyles: {
        0: { cellWidth: 8 },  1: { cellWidth: 25 }, 2: { cellWidth: 20 },
        3: { cellWidth: 28 }, 4: { cellWidth: 52 }, 5: { cellWidth: 22 },
        6: { cellWidth: 18 }, 7: { cellWidth: 25 }, 8: { cellWidth: 20 },
        9: { cellWidth: 20 },
      },
      didDrawCell: (data: any) => {
        if (data.column.index === 9 && data.section === 'body') {
          const estado = data.cell.raw as string;
          if (estado === 'Ejecutada')  { data.cell.styles.textColor = [21, 128, 61];  }
          if (estado === 'En proceso') { data.cell.styles.textColor = [59, 130, 246]; }
          if (estado === 'Abierta')    { data.cell.styles.textColor = [245, 158, 11]; }
        }
        if (data.column.index === 6 && data.section === 'body') {
          const prioridad = data.cell.raw as string;
          if (prioridad === 'Alta')  { data.cell.styles.textColor = [239, 68, 68];   }
          if (prioridad === 'Media') { data.cell.styles.textColor = [245, 158, 11];  }
          if (prioridad === 'Baja')  { data.cell.styles.textColor = [100, 116, 139]; }
        }
      }
    });

    doc.save(`seguimientos_rrhh_${new Date().toISOString().slice(0, 10)}.pdf`);
  }

  // ── Modales ────────────────────────────────────────────────────────────────
  abrirCrear() {
    this.resetForm();
    this.vistaModal = 'crear';
    this.cdr.markForCheck();
  }

  abrirEditar(r: SeguimientoRrhh) {
    this.registroSeleccionado = r;
    this.form = {
      area: r.area, mes: r.mes, anio: r.anio, fuente: r.fuente,
      areas: r.areas ?? '', descripcion: r.descripcion,
      planAccionSugerido: r.planAccionSugerido ?? '',
      factorRiesgo: r.factorRiesgo ?? '', prioridad: r.prioridad,
      responsable: r.responsable ?? '',
      fechaEjecucion:   r.fechaEjecucion   ? r.fechaEjecucion.split('T')[0]   : '',
      fechaSeguimiento: r.fechaSeguimiento ? r.fechaSeguimiento.split('T')[0] : '',
      estado: r.estado, observaciones: r.observaciones ?? '',
    };
    this.fotosEvidenciaFiles   = [];
    this.fotosSeguimientoFiles = [];
    this.previewsEvidencia     = [];
    this.previewsSeguimiento   = [];
    this.vistaModal = 'editar';
    this.cdr.markForCheck();
  }

  abrirVer(r: SeguimientoRrhh) {
    this.registroSeleccionado = r;
    this.vistaModal = 'ver';
    this.cdr.markForCheck();
  }

  abrirGaleria(fotos: SeguimientoRrhhFoto[], indice = 0) {
    this.fotosGaleria      = fotos;
    this.fotoGaleriaActual = indice;
    this.vistaModal        = 'galeria';
    this.cdr.markForCheck();
  }

  anteriorFoto() {
    this.fotoGaleriaActual = (this.fotoGaleriaActual - 1 + this.fotosGaleria.length) % this.fotosGaleria.length;
    this.cdr.markForCheck();
  }

  siguienteFoto() {
    this.fotoGaleriaActual = (this.fotoGaleriaActual + 1) % this.fotosGaleria.length;
    this.cdr.markForCheck();
  }

  cerrarModal() {
    this.vistaModal           = null;
    this.registroSeleccionado = null;
    this.mensajeError         = '';
    this.cdr.markForCheck();
  }

  // ── Guardar ────────────────────────────────────────────────────────────────
  guardar() {
    if (!this.validarForm()) return;
    this.guardando = true;

    const dto = {
      area:               this.form.area,
      mes:                +this.form.mes,
      anio:               +this.form.anio,
      fuente:             this.form.fuente,
      areas:              this.form.areas              || undefined,
      descripcion:        this.form.descripcion,
      planAccionSugerido: this.form.planAccionSugerido || undefined,
      factorRiesgo:       this.form.factorRiesgo       || undefined,
      prioridad:          this.form.prioridad,
      responsable:        this.form.responsable        || undefined,
      fechaEjecucion:     this.form.fechaEjecucion     || undefined,
      fechaSeguimiento:   this.form.fechaSeguimiento   || undefined,
      estado:             this.form.estado,
      observaciones:      this.form.observaciones      || undefined,
    };

    if (this.vistaModal === 'crear') {
      this.svc.crear(dto, this.fotosEvidenciaFiles, this.fotosSeguimientoFiles).subscribe({
        next: () => this.onExito('Seguimiento creado correctamente'),
        error: () => this.onError(),
      });
    } else if (this.vistaModal === 'editar' && this.registroSeleccionado) {
      this.svc.actualizar(this.registroSeleccionado.id, dto, this.fotosEvidenciaFiles, this.fotosSeguimientoFiles).subscribe({
        next: () => this.onExito('Seguimiento actualizado correctamente'),
        error: () => this.onError(),
      });
    }
  }

  eliminar(r: SeguimientoRrhh) {
    if (!confirm(`¿Eliminar el seguimiento #${r.id}?`)) return;
    this.svc.eliminar(r.id).subscribe({
      next: () => { this.mostrarExito('Eliminado correctamente'); this.cargar(); },
      error: () => this.mostrarError('No se pudo eliminar'),
    });
  }

  eliminarFoto(foto: SeguimientoRrhhFoto) {
    if (!confirm('¿Eliminar esta foto?')) return;
    this.svc.eliminarFoto(foto.id).subscribe({
      next: () => {
        if (this.registroSeleccionado) {
          this.registroSeleccionado.fotos = this.registroSeleccionado.fotos.filter(f => f.id !== foto.id);
        }
        this.mostrarExito('Foto eliminada');
        this.cargar();
        this.cdr.markForCheck();
      },
      error: () => this.mostrarError('No se pudo eliminar la foto'),
    });
  }

  // ── Upload fotos ───────────────────────────────────────────────────────────
  onFotosEvidencia(event: Event) {
    const files = Array.from((event.target as HTMLInputElement).files ?? []);
    this.fotosEvidenciaFiles = [...this.fotosEvidenciaFiles, ...files];
    files.forEach(f => {
      const reader = new FileReader();
      reader.onload = e => { this.previewsEvidencia.push(e.target?.result as string); this.cdr.markForCheck(); };
      reader.readAsDataURL(f);
    });
  }

  onFotosSeguimiento(event: Event) {
    const files = Array.from((event.target as HTMLInputElement).files ?? []);
    this.fotosSeguimientoFiles = [...this.fotosSeguimientoFiles, ...files];
    files.forEach(f => {
      const reader = new FileReader();
      reader.onload = e => { this.previewsSeguimiento.push(e.target?.result as string); this.cdr.markForCheck(); };
      reader.readAsDataURL(f);
    });
  }

  quitarPreviewEvidencia(i: number) {
    this.fotosEvidenciaFiles.splice(i, 1);
    this.previewsEvidencia.splice(i, 1);
    this.cdr.markForCheck();
  }

  quitarPreviewSeguimiento(i: number) {
    this.fotosSeguimientoFiles.splice(i, 1);
    this.previewsSeguimiento.splice(i, 1);
    this.cdr.markForCheck();
  }

  // ── Helpers UI ─────────────────────────────────────────────────────────────
  getFotoUrl(nombre: string)              { return this.svc.getFotoUrl(nombre); }
  getNombreMes(num: number)               { return this.meses.find(m => m.valor === num)?.nombre ?? String(num); }
  getFotosEvidencia(r: SeguimientoRrhh)   { return r.fotos.filter(f => f.tipoFoto === 'evidencia'); }
  getFotosSeguimiento(r: SeguimientoRrhh) { return r.fotos.filter(f => f.tipoFoto === 'seguimiento'); }

  getBadgeEstado(estado: string): string {
    const map: Record<string, string> = {
      'Ejecutada':  'badge-ejecutado',
      'En proceso': 'badge-proceso',
      'Abierta':    'badge-pendiente',
    };
    return map[estado] ?? 'badge-secundario';
  }

  getBadgePrioridad(p: string): string {
    const map: Record<string, string> = {
      'Alta':  'badge-alta',
      'Media': 'badge-media',
      'Baja':  'badge-baja',
    };
    return map[p] ?? 'badge-secundario';
  }

  // ── Privados ───────────────────────────────────────────────────────────────
  private validarForm(): boolean {
    if (!this.form.area.trim())        { this.mostrarError('El área es obligatoria');        return false; }
    if (!this.form.fuente)             { this.mostrarError('La fuente es obligatoria');      return false; }
    if (!this.form.descripcion.trim()) { this.mostrarError('La descripción es obligatoria'); return false; }
    return true;
  }

  private onExito(msg: string) {
    this.guardando = false;
    this.cerrarModal();
    this.mostrarExito(msg);
    this.cargar();
  }

  private onError() {
    this.guardando = false;
    this.mostrarError('Error al guardar. Intente nuevamente.');
    this.cdr.markForCheck();
  }

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

  private resetForm() {
    this.form = {
      area: '', mes: new Date().getMonth() + 1, anio: new Date().getFullYear(),
      fuente: '', areas: '', descripcion: '', planAccionSugerido: '',
      factorRiesgo: '', prioridad: 'Media', responsable: '',
      fechaEjecucion: '', fechaSeguimiento: '', estado: 'Abierta', observaciones: '',
    };
    this.fotosEvidenciaFiles   = [];
    this.fotosSeguimientoFiles = [];
    this.previewsEvidencia     = [];
    this.previewsSeguimiento   = [];
    this.mensajeError          = '';
  }
}