import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IncidentesService } from '../../services/incidentes.service';
import { PermisosService }   from '../../services/permisos.service';
import { PdfService }        from '../../services/pdf.service';
import { environment }       from '../../../environments/environment';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-incidentes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './incidentes.html',
  styleUrls: ['./incidentes.scss']
})
export class IncidentesComponent implements OnInit {

  incidentes:            any[] = [];
  incidentesFiltrados:   any[] = [];
  incidenteSeleccionado: any   = null;
  mostrarModalRevisar        = false;

  filtroEstado     = '';
  filtroTipo       = '';
  filtroBusqueda   = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  revisadoPor  = '';
  observacion  = '';

  // ✅ URL dinámica desde environment
  baseUrl = environment.fotosUrl.replace('/fotos', '');

  readonly tiposIncidente = [
    { value: 'DañoMecanico', label: '🔧 Daño mecánico' },
    { value: 'Averia',       label: '⚠️ Avería' },
    { value: 'Trancon',      label: '🚗 Trancón' },
    { value: 'CierreVia',    label: '🚧 Cierre de vía' },
    { value: 'Accidente',    label: '💥 Accidente' },
    { value: 'Otro',         label: '📋 Otro' },
  ];

  get puedeVer():    boolean { return this.permisosService.puedeVer('incidentes'); }
  get puedeEditar(): boolean { return this.permisosService.puedeEditar('incidentes'); }
  get pendientes():  number  { return this.incidentes.filter(i => i.estado === 'Pendiente').length; }

  constructor(
    private incidentesService: IncidentesService,
    private permisosService:   PermisosService,
    private pdfService:        PdfService
  ) {}

  ngOnInit(): void { this.cargarIncidentes(); }

  cargarIncidentes() {
    this.incidentesService.obtenerIncidentes().subscribe({
      next: (data) => { this.incidentes = data; this.aplicarFiltros(); },
      error: (err)  => console.error(err)
    });
  }

  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.incidentesFiltrados = this.incidentes.filter(i => {
      const okEstado = !this.filtroEstado || i.estado === this.filtroEstado;
      const okTipo   = !this.filtroTipo   || i.tipoIncidente === this.filtroTipo;
      const fecha    = new Date(i.fechaReporte);
      const okDesde  = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta  = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq   = !q ||
        i.conductor?.nombre?.toLowerCase().includes(q) ||
        i.vehiculo?.placa?.toLowerCase().includes(q)   ||
        i.descripcionDetallada?.toLowerCase().includes(q) ||
        i.ubicacionGPS?.toLowerCase().includes(q);
      return okEstado && okTipo && okDesde && okHasta && okBusq;
    });
  }

  limpiarFiltros() {
    this.filtroEstado = ''; this.filtroTipo = '';
    this.filtroBusqueda = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  verDetalle(incidente: any) {
    this.incidenteSeleccionado = incidente;
    this.mostrarModalRevisar   = false;
    this.revisadoPor           = '';
    this.observacion           = '';
  }

  obtenerFotos(fotosStr: string): string[] {
    if (!fotosStr) return [];
    return fotosStr.split(',').filter(f => f.trim() !== '');
  }

  // ✅ Contacto manual — el supervisor puede llamar al conductor si necesita
  getLinkWhatsApp(telefono: string): string {
    return `https://wa.me/${telefono.replace(/\D/g, '')}`;
  }

  abrirRevisar() {
    this.mostrarModalRevisar = true;
    this.revisadoPor = ''; this.observacion = '';
  }

  confirmarRevision() {
    if (!this.revisadoPor) { alert('Ingrese su nombre'); return; }
    this.incidentesService.marcarRevisado(
      this.incidenteSeleccionado.id,
      { revisadoPor: this.revisadoPor, observacion: this.observacion }
    ).subscribe({
      next: () => {
        this.mostrarModalRevisar   = false;
        this.incidenteSeleccionado = null;
        this.cargarIncidentes();
      },
      error: (err) => console.error(err)
    });
  }

  getBadgeTipo(tipo: string): string {
    const t = this.tiposIncidente.find(t => t.value === tipo);
    return t ? t.label : tipo;
  }

  getBadgeEstado(estado: string): string {
    return estado === 'Pendiente' ? 'badge-pendiente' : 'badge-revisado';
  }

  cerrarDetalle() { this.incidenteSeleccionado = null; this.mostrarModalRevisar = false; }

  async descargarPDF(incidente: any) { await this.pdfService.generarPDFIncidente(incidente); }

  exportarExcel() {
    const datos = this.incidentesFiltrados.map(i => ({
      'ID':                   i.id,
      'Fecha':                new Date(i.fechaReporte).toLocaleString(),
      'Conductor':            i.conductor?.nombre    ?? '-',
      'Vehículo':             i.vehiculo?.placa      ?? '-',
      'Tipo':                 this.getBadgeTipo(i.tipoIncidente),
      'Descripción':          i.descripcionDetallada ?? '-',
      'Ubicación GPS':        i.ubicacionGPS         || '-',
      'Estado':               i.estado,
      'Revisado por':         i.revisadoPor          || '-',
      'Fecha revisión':       i.fechaRevision ? new Date(i.fechaRevision).toLocaleString() : '-',
      'Observación revisión': i.observacionRevision  || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      {wch:6},{wch:20},{wch:22},{wch:12},{wch:18},{wch:40},
      {wch:25},{wch:12},{wch:18},{wch:20},{wch:30}
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Incidentes');
    XLSX.writeFile(libro, `incidentes_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  exportarPDF() {
    const doc   = new jsPDF('landscape');
    const VERDE = [21, 128, 61] as [number, number, number];
    doc.setFillColor(...VERDE); doc.rect(0, 0, 297, 20, 'F');
    doc.setFontSize(14); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold');
    doc.text('REPORTE DE INCIDENTES EN RUTA', 148, 13, { align: 'center' });
    doc.setFontSize(9); doc.setTextColor(80,80,80); doc.setFont('helvetica','normal');
    doc.text(`Generado: ${new Date().toLocaleString()}`, 14, 28);
    doc.text(`Total: ${this.incidentesFiltrados.length}`, 14, 35);
    doc.text(`Pendientes: ${this.pendientes}`, 80, 35);
    autoTable(doc, {
      startY: 40,
      head: [['#','Fecha','Conductor','Vehículo','Tipo','Descripción','GPS','Estado','Revisado por']],
      body: this.incidentesFiltrados.map(i => [
        i.id, new Date(i.fechaReporte).toLocaleDateString(),
        i.conductor?.nombre ?? '-', i.vehiculo?.placa ?? '-',
        this.getBadgeTipo(i.tipoIncidente),
        (i.descripcionDetallada ?? '-').slice(0,50) + (i.descripcionDetallada?.length > 50 ? '...' : ''),
        i.ubicacionGPS || '-', i.estado, i.revisadoPor || '-',
      ]),
      headStyles: { fillColor: VERDE, textColor: [255,255,255], fontStyle: 'bold', fontSize: 8 },
      bodyStyles: { fontSize: 8 },
      alternateRowStyles: { fillColor: [245,245,245] },
      columnStyles: {
        0:{cellWidth:10,halign:'center'},1:{cellWidth:22},2:{cellWidth:28},
        3:{cellWidth:18},4:{cellWidth:22},5:{cellWidth:55},
        6:{cellWidth:30},7:{cellWidth:20,halign:'center'},8:{cellWidth:25}
      }
    });
    const n = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= n; i++) {
      doc.setPage(i); doc.setDrawColor(...VERDE); doc.setLineWidth(0.3);
      doc.line(14, 200, 283, 200); doc.setFontSize(7); doc.setTextColor(120,120,120);
      doc.text('Sistema de Gestión de Flota', 148, 204, { align: 'center' });
      doc.text(`Página ${i} de ${n}`, 283, 204, { align: 'right' });
    }
    doc.save(`incidentes_${new Date().toISOString().slice(0,10)}.pdf`);
  }
}