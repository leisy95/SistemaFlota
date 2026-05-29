import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InspeccionesService } from '../../services/inspecciones.service';
import { PermisosService }     from '../../services/permisos.service';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import QRCode from 'qrcode';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-inspecciones-historial',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inspecciones-historial.html',
  styleUrls: ['./inspecciones-historial.scss']
})
export class InspeccionesHistorialComponent implements OnInit {

  historial:           any[] = [];
  historialFiltrado:   any[] = [];
  historialPaginado:   any[] = [];
  detalleSeleccionado: any   = null;
  paginaActual   = 1;
  itemsPorPagina = 10;

  // Filtros
  filtroBusqueda   = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  get puedeVer(): boolean { return this.permisosService.puedeVer('inspecciones'); }

  constructor(
    private inspeccionesService: InspeccionesService,
    private permisosService:     PermisosService
  ) {}

  ngOnInit(): void { this.obtenerHistorial(); }

  obtenerHistorial() {
    this.inspeccionesService.obtenerHistorial().subscribe({
      next: (data: any) => { this.historial = data; this.aplicarFiltros(); },
      error: (err: any)  => console.error(err)
    });
  }

  // ── Filtros ───────────────────────────────────────────────────────────────────
  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.historialFiltrado = this.historial.filter(i => {
      const fecha   = new Date(i.fecha);
      const okDesde = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq  = !q ||
        i.conductor?.nombre?.toLowerCase().includes(q) ||
        i.vehiculo?.placa?.toLowerCase().includes(q);
      return okDesde && okHasta && okBusq;
    });
    this.paginaActual = 1;
    this.actualizarPaginado();
  }

  limpiarFiltros() {
    this.filtroBusqueda = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  private actualizarPaginado() {
    const inicio = (this.paginaActual - 1) * this.itemsPorPagina;
    this.historialPaginado = this.historialFiltrado.slice(inicio, inicio + this.itemsPorPagina);
  }

  get totalPaginas(): number {
    return Math.ceil(this.historialFiltrado.length / this.itemsPorPagina);
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) { this.paginaActual++; this.actualizarPaginado(); }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) { this.paginaActual--; this.actualizarPaginado(); }
  }

  verDetalle(id: number) {
    this.inspeccionesService.obtenerDetalle(id).subscribe({
      next:  (data: any) => this.detalleSeleccionado = data,
      error: (err: any)  => { console.error(err); alert('Error cargando detalle'); }
    });
  }

  private cargarImagen(src: string): Promise<HTMLImageElement | null> {
    return new Promise(resolve => {
      const img = new Image();
      img.src = src;
      img.onload  = () => resolve(img);
      img.onerror = () => resolve(null);
    });
  }

  descargarPDF(inspeccion: any) {
    this.inspeccionesService.obtenerDetalle(inspeccion.id).subscribe({
      next:  async (detalle: any) => await this.generarPDF(detalle),
      error: (err: any) => { console.error(err); alert('Error cargando detalle para PDF'); }
    });
  }

  private async generarPDF(inspeccion: any) {
    const doc   = new jsPDF();
    const VERDE = [21,128,61]   as [number,number,number];
    const GRIS  = [60,60,60]    as [number,number,number];
    const CLARO = [245,245,245] as [number,number,number];
    const ROJO  = [185,28,28]   as [number,number,number];

    const qrData  = [`Inspección: ${inspeccion.id}`,`Vehículo: ${inspeccion.vehiculo?.placa ?? '-'}`,`Conductor: ${inspeccion.conductor?.nombre ?? '-'}`,`Fecha: ${new Date(inspeccion.fecha).toLocaleString()}`].join('\n');
    const qrImage = await QRCode.toDataURL(qrData, { width: 120, margin: 1 });

    const [logo, fotoOdometro, firma] = await Promise.all([
      this.cargarImagen('http://localhost:4200/assets/logo.png'),
      inspeccion.fotoOdometro?.trim() ? this.cargarImagen(`http://localhost:5214/inspecciones/${inspeccion.fotoOdometro}`) : Promise.resolve(null),
      inspeccion.firmaCondutor?.trim() ? this.cargarImagen(`http://localhost:5214/inspecciones/${inspeccion.firmaCondutor}`) : Promise.resolve(null)
    ]);

    doc.setFillColor(...VERDE); doc.rect(0,0,210,42,'F');
    if (logo) doc.addImage(logo,'PNG',5,4,32,32);
    doc.setFontSize(18); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold');
    doc.text('REPORTE DE INSPECCIÓN',105,18,{align:'center'});
    doc.setFontSize(10); doc.setFont('helvetica','normal');
    doc.text('Sistema de Gestión de Flota',105,27,{align:'center'});
    doc.addImage(qrImage,'PNG',175,3,32,32);
    doc.setFontSize(6); doc.setTextColor(200,200,200);
    doc.text('Escanear QR',191,37,{align:'center'});

    const estadoGeneral = inspeccion.estadoGeneral ?? 'APROBADO';
    const esAprobado    = estadoGeneral === 'APROBADO';
    doc.setFillColor(...(esAprobado ? VERDE : ROJO));
    doc.roundedRect(14,47,80,10,2,2,'F');
    doc.setFontSize(9); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold');
    doc.text(esAprobado ? '✔ APROBADO' : '✘ RECHAZADO',54,54,{align:'center'});

    doc.setFont('helvetica','normal'); doc.setFontSize(10); doc.setTextColor(...GRIS);
    const datos = [['ID',`${inspeccion.id}`],['Fecha',new Date(inspeccion.fecha).toLocaleString()],['Conductor',inspeccion.conductor?.nombre??'-'],['Vehículo',inspeccion.vehiculo?.placa??'-'],['Kilometraje',`${inspeccion.kilometraje} km`]];
    let y = 63;
    datos.forEach(([label,valor]) => {
      doc.setFont('helvetica','bold'); doc.setTextColor(...VERDE); doc.text(`${label}:`,14,y);
      doc.setFont('helvetica','normal'); doc.setTextColor(...GRIS); doc.text(valor,50,y); y+=9;
    });

    if (fotoOdometro) {
      doc.addImage(fotoOdometro,'JPEG',135,47,60,50);
      doc.setFontSize(7); doc.setTextColor(120,120,120);
      doc.text('Foto odómetro',165,100,{align:'center'});
    }

    doc.setDrawColor(...VERDE); doc.setLineWidth(0.5); doc.line(14,108,196,108);

    autoTable(doc, {
      startY: 112,
      head: [['#','Ítem','Estado','Observación']],
      body: inspeccion.detalles?.map((d:any,i:number)=>[i+1,d.nombreItem??d.checklistItemId,d.estado,d.observacion||'-'])||[],
      headStyles: {fillColor:VERDE,textColor:[255,255,255],fontStyle:'bold',fontSize:9},
      bodyStyles: {fontSize:9,textColor:GRIS},
      alternateRowStyles: {fillColor:CLARO},
      columnStyles: {0:{cellWidth:10,halign:'center'},1:{cellWidth:70},2:{cellWidth:30,halign:'center'},3:{cellWidth:76}},
      didDrawCell: (data:any) => {
        if (data.column.index===2 && data.section==='body') {
          const estado=data.cell.text[0];
          const x=data.cell.x+2,y=data.cell.y+1.5,w=data.cell.width-4,h=data.cell.height-3;
          if (estado==='Cumple') doc.setFillColor(...VERDE);
          else if (estado==='No cumple') doc.setFillColor(...ROJO);
          else doc.setFillColor(107,114,128);
          doc.roundedRect(x,y,w,h,1.5,1.5,'F');
          doc.setTextColor(255,255,255); doc.setFontSize(8);
          doc.text(estado,x+w/2,y+h/2+0.5,{align:'center'});
        }
      }
    });

    if (firma) {
      const firmaY=(doc as any).lastAutoTable?.finalY??200;
      if (firmaY+60>280) doc.addPage();
      doc.setFontSize(10); doc.setFont('helvetica','bold'); doc.setTextColor(...VERDE);
      doc.text('Firma del conductor:',14,firmaY+15);
      doc.setFillColor(...CLARO); doc.roundedRect(14,firmaY+18,80,35,3,3,'F');
      doc.addImage(firma,'PNG',16,firmaY+20,76,30);
      doc.setDrawColor(...VERDE); doc.setLineWidth(0.5); doc.line(14,firmaY+53,94,firmaY+53);
      doc.setFontSize(9); doc.setFont('helvetica','normal'); doc.setTextColor(...GRIS);
      doc.text(inspeccion.conductor?.nombre??'',54,firmaY+58,{align:'center'});
    }

    const n=(doc as any).internal.getNumberOfPages();
    for (let i=1;i<=n;i++) {
      doc.setPage(i); doc.setDrawColor(...VERDE); doc.setLineWidth(0.3);
      doc.line(14,285,196,285); doc.setFontSize(7); doc.setTextColor(120,120,120); doc.setFont('helvetica','normal');
      doc.text('Sistema de Gestión de Flota — Documento generado automáticamente',105,289,{align:'center'});
      doc.text(`Página ${i} de ${n}`,196,289,{align:'right'});
      doc.text(new Date().toLocaleString(),14,289);
    }
    doc.save(`inspeccion-${inspeccion.id}.pdf`);
  }

  exportarExcel() {
    const datos = this.historialFiltrado.map(i => ({
      'ID':          i.id,
      'Fecha':       new Date(i.fecha).toLocaleString(),
      'Conductor':   i.conductor?.nombre ?? '-',
      'Vehículo':    i.vehiculo?.placa   ?? '-',
      'Kilometraje': `${i.kilometraje} km`,
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [{wch:8},{wch:20},{wch:25},{wch:15},{wch:15}];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Inspecciones');
    XLSX.writeFile(libro, `inspecciones_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}