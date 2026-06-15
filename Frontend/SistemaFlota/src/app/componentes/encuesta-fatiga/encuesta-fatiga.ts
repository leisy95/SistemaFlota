import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EncuestaFatigaService } from '../../services/encuesta-fatiga.service';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { PermisosService } from '../../services/permisos.service';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-encuesta-fatiga',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './encuesta-fatiga.html',
  styleUrls: ['./encuesta-fatiga.scss']
})
export class EncuestaFatigaComponent implements OnInit {

  encuestas: any[] = [];
  conductores: any[] = [];
  vehiculos: any[] = [];
  estadisticas: any = null;

  paginaActual = 1;
  porPagina = 10;
  totalRegistros = 0;
  totalPaginas = 0;
  filtroBusqueda = '';
  filtroResultado = '';

  mostrarFormulario = false;
  resultadoActual: any = null;
  cargando = false;

  form = {
    conductorId: 0, vehiculoId: 0,
    durmioMenos7Horas: false, sienteCansancio: false,
    despertoVariasVeces: false, medicamentoSueno: false,
    dificultadConcentracion: false,
    otraObservacion: '', registradoPor: '', observaciones: ''
  };

  preguntasExtra: { pregunta: string; respuesta: boolean }[] = [];

  readonly preguntas = [
    { key: 'durmioMenos7Horas', texto: '¿Durmió menos de 7 horas?', alerta: 'Descanso insuficiente' },
    { key: 'sienteCansancio', texto: '¿Se siente cansado o con sueño?', alerta: 'Cansancio activo' },
    { key: 'despertoVariasVeces', texto: '¿Se despertó varias veces durante la noche o no descansó bien?', alerta: 'Sueño interrumpido' },
    { key: 'medicamentoSueno', texto: '¿Está tomando algún medicamento que le produzca sueño?', alerta: 'Medicamento activo' },
    { key: 'dificultadConcentracion', texto: '¿Siente dificultad para concentrarse en este momento?', alerta: 'Concentración reducida' },
  ];

  // ── Permisos ─────────────────────────────────────────────────────────────────
  get puedeCrear(): boolean { return this.permisosService.puedeCrear('encuesta-fatiga'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('encuesta-fatiga'); }

  constructor(
    private encuestaService: EncuestaFatigaService,
    private conductoresService: ConductoresService,
    private vehiculosService: VehiculosService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.cargarEncuestas();
    this.cargarConductores();
    this.cargarVehiculos();
    this.cargarEstadisticas();
  }

  cargarEncuestas() {
    this.encuestaService.obtenerEncuestas({
      pagina: this.paginaActual,
      porPagina: this.porPagina,
      buscar: this.filtroBusqueda || undefined,
      resultado: this.filtroResultado || undefined
    }).subscribe({
      next: (res: any) => {
        if (Array.isArray(res)) {
          this.encuestas = res;
          this.totalRegistros = res.length;
          this.totalPaginas = 1;
        } else {
          this.encuestas = res.data ?? [];
          this.totalRegistros = res.total ?? 0;
          this.totalPaginas = res.totalPaginas ?? 1;
        }
      },
      error: (err) => console.error(err)
    });
  }
  aplicarFiltros() {
  this.paginaActual = 1;
  this.cargarEncuestas();
}

limpiarFiltros() {
  this.filtroBusqueda  = '';
  this.filtroResultado = '';
  this.paginaActual    = 1;
  this.cargarEncuestas();
}

paginaSiguiente() {
  if (this.paginaActual < this.totalPaginas) {
    this.paginaActual++;
    this.cargarEncuestas();
  }
}

paginaAnterior() {
  if (this.paginaActual > 1) {
    this.paginaActual--;
    this.cargarEncuestas();
  }
}

  cargarConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next: (data) => this.conductores = data,
      error: (err) => console.error(err)
    });
  }

  cargarVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err) => console.error(err)
    });
  }

  cargarEstadisticas() {
    this.encuestaService.obtenerEstadisticas().subscribe({
      next: (data) => this.estadisticas = data,
      error: (err) => console.error(err)
    });
  }

  nuevaEncuesta() {
    this.form = {
      conductorId: 0, vehiculoId: 0,
      durmioMenos7Horas: false, sienteCansancio: false,
      despertoVariasVeces: false, medicamentoSueno: false,
      dificultadConcentracion: false,
      otraObservacion: '', registradoPor: '', observaciones: ''
    };
    this.preguntasExtra = [];
    this.resultadoActual = null;
    this.mostrarFormulario = true;
  }

  getPregunta(key: string): boolean { return (this.form as any)[key]; }
  setPregunta(key: string, valor: boolean) { (this.form as any)[key] = valor; }

  agregarPreguntaExtra() { this.preguntasExtra.push({ pregunta: '', respuesta: false }); }
  eliminarPreguntaExtra(i: number) { this.preguntasExtra.splice(i, 1); }

  get respuestasSi(): number {
    return [
      this.form.durmioMenos7Horas, this.form.sienteCansancio,
      this.form.despertoVariasVeces, this.form.medicamentoSueno,
      this.form.dificultadConcentracion
    ].filter(v => v).length;
  }

  get resultadoPreview(): string { return this.respuestasSi >= 2 ? 'No Apto' : 'Apto'; }

  get alertasActivas(): string[] {
    return this.preguntas.filter(p => (this.form as any)[p.key]).map(p => p.alerta);
  }

  guardar() {
    if (!this.form.conductorId) { alert('Seleccione un conductor'); return; }
    if (!this.form.vehiculoId) { alert('Seleccione un vehículo'); return; }
    if (!this.form.registradoPor.trim()) { alert('Ingrese quién registra'); return; }

    const extrasTexto = this.preguntasExtra
      .filter(p => p.pregunta.trim() !== '')
      .map(p => `• ${p.pregunta}: ${p.respuesta ? 'Sí' : 'No'}`)
      .join('\n');

    const datos = {
      ...this.form,
      otraObservacion: this.form.otraObservacion +
        (extrasTexto ? '\nPreguntas adicionales:\n' + extrasTexto : '')
    };

    this.cargando = true;
    this.encuestaService.crear(datos).subscribe({
      next: (data: any) => {
        this.resultadoActual = data;
        this.mostrarFormulario = false;
        this.cargando = false;
        this.preguntasExtra = [];
        this.cargarEncuestas();
        this.cargarEstadisticas();
      },
      error: (err) => { console.error(err); this.cargando = false; alert('Error guardando encuesta'); }
    });
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar esta encuesta?')) return;
    this.encuestaService.eliminar(id).subscribe({
      next: () => this.cargarEncuestas(),
      error: (err) => console.error(err)
    });
  }

  cerrarResultado() { this.resultadoActual = null; }

  getBadgeResultado(resultado: string): string {
    return resultado === 'Apto' ? 'badge-apto' : 'badge-no-apto';
  }

  contarSi(e: any): number {
    return [e.durmioMenos7Horas, e.sienteCansancio, e.despertoVariasVeces,
    e.medicamentoSueno, e.dificultadConcentracion].filter(v => v).length;
  }

  generarPDFIndividual(e: any) {
    const doc = new jsPDF();
    const VERDE = [21, 128, 61] as [number, number, number];
    const ROJO = [185, 28, 28] as [number, number, number];
    const COLOR = e.resultado === 'Apto' ? VERDE : ROJO;

    doc.setFillColor(...COLOR);
    doc.rect(0, 0, 210, 28, 'F');
    doc.setFontSize(16); doc.setTextColor(255, 255, 255); doc.setFont('helvetica', 'bold');
    doc.text('ENCUESTA PREOPERACIONAL DE FATIGA', 105, 14, { align: 'center' });
    doc.setFontSize(10);
    doc.text(`Resultado: ${e.resultado}`, 105, 22, { align: 'center' });

    doc.setTextColor(30, 30, 30); doc.setFontSize(11); doc.setFont('helvetica', 'bold');
    doc.text('INFORMACIÓN DEL CONDUCTOR', 14, 40);
    doc.setFont('helvetica', 'normal'); doc.setFontSize(10);
    doc.text(`Conductor:      ${e.conductor?.nombre ?? '-'}`, 14, 48);
    doc.text(`Licencia:       ${e.conductor?.licencia ?? '-'}`, 14, 55);
    doc.text(`Vehículo:       ${e.vehiculo?.placa ?? '-'}`, 14, 62);
    doc.text(`Fecha:          ${new Date(e.fecha).toLocaleString()}`, 14, 69);
    doc.text(`Registrado por: ${e.registradoPor ?? '-'}`, 14, 76);

    doc.setFillColor(...COLOR);
    doc.roundedRect(140, 38, 55, 20, 3, 3, 'F');
    doc.setTextColor(255, 255, 255); doc.setFontSize(14); doc.setFont('helvetica', 'bold');
    doc.text(e.resultado === 'Apto' ? '✓ APTO' : '✗ NO APTO', 167, 52, { align: 'center' });

    doc.setTextColor(30, 30, 30); doc.setFontSize(11); doc.setFont('helvetica', 'bold');
    doc.text('RESPUESTAS', 14, 90);

    const preguntas = [
      { texto: '¿Durmió menos de 7 horas?', valor: e.durmioMenos7Horas },
      { texto: '¿Se siente cansado o con sueño?', valor: e.sienteCansancio },
      { texto: '¿Se despertó varias veces durante la noche?', valor: e.despertoVariasVeces },
      { texto: '¿Toma medicamento que produzca sueño?', valor: e.medicamentoSueno },
      { texto: '¿Dificultad para concentrarse en este momento?', valor: e.dificultadConcentracion },
    ];

    autoTable(doc, {
      startY: 95,
      head: [['Pregunta', 'Respuesta']],
      body: preguntas.map(p => [p.texto, p.valor ? '✗ SÍ' : '✓ NO']),
      headStyles: { fillColor: [15, 23, 42], textColor: [255, 255, 255], fontStyle: 'bold', fontSize: 10 },
      bodyStyles: { fontSize: 10 },
      alternateRowStyles: { fillColor: [245, 245, 245] },
      didDrawCell: (data: any) => {
        if (data.column.index === 1 && data.section === 'body') {
          const val = data.cell.text[0];
          const x = data.cell.x + 1, y = data.cell.y + 1, w = data.cell.width - 2, h = data.cell.height - 2;
          if (val.includes('SÍ')) doc.setFillColor(254, 226, 226);
          else doc.setFillColor(220, 252, 231);
          doc.roundedRect(x, y, w, h, 1, 1, 'F');
          doc.setTextColor(val.includes('SÍ') ? 185 : 21, val.includes('SÍ') ? 28 : 128, val.includes('SÍ') ? 28 : 61);
          doc.setFontSize(9);
          doc.text(val, x + w / 2, y + h / 2 + 1, { align: 'center' });
        }
      },
      columnStyles: { 0: { cellWidth: 140 }, 1: { cellWidth: 40, halign: 'center' } }
    });

    const finalY = (doc as any).lastAutoTable.finalY + 10;
    if (e.otraObservacion) {
      doc.setFont('helvetica', 'bold'); doc.setFontSize(10); doc.setTextColor(30, 30, 30);
      doc.text('Observación adicional:', 14, finalY);
      doc.setFont('helvetica', 'normal');
      doc.text(doc.splitTextToSize(e.otraObservacion, 180), 14, finalY + 7);
    }
    if (e.observaciones) {
      const offsetY = e.otraObservacion ? finalY + 30 : finalY;
      doc.setFont('helvetica', 'bold'); doc.setFontSize(10); doc.setTextColor(30, 30, 30);
      doc.text('Observaciones generales:', 14, offsetY);
      doc.setFont('helvetica', 'normal');
      doc.text(e.observaciones, 14, offsetY + 7);
    }

    doc.setDrawColor(...COLOR); doc.setLineWidth(0.5); doc.line(14, 280, 196, 280);
    doc.setFontSize(8); doc.setTextColor(120, 120, 120);
    doc.text('Sistema de Gestión de Flota — Encuesta Preoperacional', 105, 285, { align: 'center' });
    doc.save(`encuesta_fatiga_${e.conductor?.nombre ?? 'conductor'}_${new Date().toISOString().slice(0, 10)}.pdf`);
  }

  exportarExcel() {
    const datos = this.encuestas.map(e => ({
      'ID': e.id,
      'Fecha': new Date(e.fecha).toLocaleString(),
      'Conductor': e.conductor?.nombre ?? '-',
      'Licencia': e.conductor?.licencia ?? '-',
      'Vehículo': e.vehiculo?.placa ?? '-',
      'Durmió menos 7h': e.durmioMenos7Horas ? 'Sí' : 'No',
      'Siente cansancio': e.sienteCansancio ? 'Sí' : 'No',
      'Despertó varias veces': e.despertoVariasVeces ? 'Sí' : 'No',
      'Medicamento sueño': e.medicamentoSueno ? 'Sí' : 'No',
      'Dificultad concentración': e.dificultadConcentracion ? 'Sí' : 'No',
      'Otra observación': e.otraObservacion || '-',
      'Resultado': e.resultado,
      'Registrado por': e.registradoPor || '-',
      'Observaciones': e.observaciones || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6 }, { wch: 20 }, { wch: 22 }, { wch: 15 }, { wch: 12 }, { wch: 15 }, { wch: 15 },
      { wch: 20 }, { wch: 15 }, { wch: 22 }, { wch: 25 }, { wch: 12 }, { wch: 18 }, { wch: 30 }
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Encuesta Fatiga');
    XLSX.writeFile(libro, `encuesta_fatiga_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}