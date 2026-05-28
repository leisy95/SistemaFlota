import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MantenimientoService } from '../../services/mantenimiento.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { PdfService } from '../../services/pdf.service';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-mantenimiento',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mantenimiento.html',
  styleUrls: ['./mantenimiento.scss']
})

export class MantenimientoComponent implements OnInit {

  mantenimientos:    any[] = [];
  vehiculos:         any[] = [];
  proximos:          any[] = [];
  seleccionado:      any   = null;
  mostrarModal       = false;
  mostrarDetalle     = false;
  mostrarFinalizar   = false;
  editando           = false;
  editandoId:        number | null = null;

  filtroVehiculo = 0;
  filtroEstado   = '';

  fotosSeleccionadas: File[]   = [];
  fotosPreview:       string[] = [];

  fechaSalida      = '';
  observacionFinal = '';

  form = {
    vehiculoId:           0,
    tipoMantenimiento:    '',
    fechaEntrada:         new Date().toISOString().slice(0, 16),
    kilometrajeEntrada:   0,
    nombreTaller:         '',
    tecnicoResponsable:   '',
    telefonoTaller:       '',
    trabajosRealizados:   '',
    repuestosUtilizados:  '',
    costoManoObra:        0,
    costoRepuestos:       0,
    observaciones:        '',
    kilometrajeSiguiente: 0,
    fechaSiguiente:       ''
  };

  readonly tiposMantenimiento = [
    'Preventivo', 'Correctivo', 'Predictivo',
    'Cambio de aceite', 'Cambio de frenos',
    'Cambio de llantas', 'Revisión general',
    'Mantenimiento eléctrico', 'Mantenimiento de suspensión', 'Otro'
  ];

  constructor(
    private mantenimientoService: MantenimientoService,
    private vehiculosService:     VehiculosService,
    private pdfService:           PdfService
  ) {}

  ngOnInit(): void {
    this.cargarMantenimientos();
    this.cargarVehiculos();
    this.cargarProximos();
  }

  cargarMantenimientos() {
    this.mantenimientoService.obtenerTodos().subscribe({
      next: (data) => this.mantenimientos = data,
      error: (err) => console.error(err)
    });
  }

  cargarVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err) => console.error(err)
    });
  }

  cargarProximos() {
    this.mantenimientoService.obtenerProximos().subscribe({
      next: (data) => this.proximos = data,
      error: (err) => console.error(err)
    });
  }

  get mantenimientosFiltrados(): any[] {
    return this.mantenimientos.filter(m => {
      const okVehiculo = !this.filtroVehiculo || m.vehiculo?.id === this.filtroVehiculo;
      const okEstado   = !this.filtroEstado   || m.estado === this.filtroEstado;
      return okVehiculo && okEstado;
    });
  }

  get enTaller(): number {
    return this.mantenimientos.filter(m => m.estado === 'EnTaller').length;
  }

  get costoTotal(): number {
    return this.mantenimientos.reduce((sum, m) => sum + (m.costoTotal || 0), 0);
  }

  seleccionarFotos(event: any) {
    const archivos = Array.from(event.target.files) as File[];
    if (archivos.length + this.fotosSeleccionadas.length > 5) {
      alert('Máximo 5 fotos'); return;
    }
    archivos.forEach(archivo => {
      this.fotosSeleccionadas.push(archivo);
      const reader = new FileReader();
      reader.onload = (e: any) => this.fotosPreview.push(e.target.result);
      reader.readAsDataURL(archivo);
    });
  }

  eliminarFoto(index: number) {
    this.fotosSeleccionadas.splice(index, 1);
    this.fotosPreview.splice(index, 1);
  }

  obtenerFotos(fotosStr: string): string[] {
    if (!fotosStr) return [];
    return fotosStr.split(',').filter(f => f.trim() !== '');
  }

  nuevoMantenimiento() {
    this.editando   = false;
    this.editandoId = null;
    this.form = {
      vehiculoId: 0, tipoMantenimiento: '',
      fechaEntrada: new Date().toISOString().slice(0, 16),
      kilometrajeEntrada: 0, nombreTaller: '',
      tecnicoResponsable: '', telefonoTaller: '',
      trabajosRealizados: '', repuestosUtilizados: '',
      costoManoObra: 0, costoRepuestos: 0,
      observaciones: '', kilometrajeSiguiente: 0,
      fechaSiguiente: ''
    };
    this.fotosSeleccionadas = [];
    this.fotosPreview       = [];
    this.mostrarModal       = true;
  }

  guardar() {
    if (!this.form.vehiculoId)         { alert('Seleccione un vehículo'); return; }
    if (!this.form.tipoMantenimiento)  { alert('Seleccione el tipo'); return; }
    if (!this.form.nombreTaller)       { alert('Ingrese el nombre del taller'); return; }
    if (!this.form.trabajosRealizados) { alert('Ingrese los trabajos realizados'); return; }

    const formData = new FormData();
    formData.append('VehiculoId',          this.form.vehiculoId.toString());
    formData.append('TipoMantenimiento',   this.form.tipoMantenimiento);
    formData.append('FechaEntrada',        this.form.fechaEntrada);
    formData.append('KilometrajeEntrada',  this.form.kilometrajeEntrada.toString());
    formData.append('NombreTaller',        this.form.nombreTaller);
    formData.append('TecnicoResponsable',  this.form.tecnicoResponsable);
    formData.append('TelefonoTaller',      this.form.telefonoTaller);
    formData.append('TrabajosRealizados',  this.form.trabajosRealizados);
    formData.append('RepuestosUtilizados', this.form.repuestosUtilizados);
    formData.append('CostoManoObra',       this.form.costoManoObra.toString());
    formData.append('CostoRepuestos',      this.form.costoRepuestos.toString());
    formData.append('Observaciones',       this.form.observaciones);
    formData.append('Estado',             'EnTaller');

    if (this.form.kilometrajeSiguiente)
      formData.append('KilometrajeSiguiente', this.form.kilometrajeSiguiente.toString());
    if (this.form.fechaSiguiente)
      formData.append('FechaSiguiente', this.form.fechaSiguiente);

    this.fotosSeleccionadas.forEach(foto => formData.append('Fotos', foto));

    if (this.editando && this.editandoId) {
      this.mantenimientoService.editar(this.editandoId, formData).subscribe({
        next: () => { this.cargarMantenimientos(); this.cerrarModal(); },
        error: (err) => console.error(err)
      });
    } else {
      this.mantenimientoService.crear(formData).subscribe({
        next: () => { this.cargarMantenimientos(); this.cerrarModal(); },
        error: (err) => console.error(err)
      });
    }
  }

  verDetalle(m: any) {
    this.seleccionado     = m;
    this.mostrarDetalle   = true;
    this.mostrarFinalizar = false;
  }

  abrirFinalizar(m: any) {
    this.seleccionado     = m;
    this.mostrarDetalle   = true;
    this.mostrarFinalizar = true;
    this.fechaSalida      = new Date().toISOString().slice(0, 16);
    this.observacionFinal = '';
  }

  confirmarFinalizar() {
    this.mantenimientoService.finalizar(
      this.seleccionado.id,
      { fechaSalida: this.fechaSalida, observacionesFinal: this.observacionFinal }
    ).subscribe({
      next: () => {
        this.cerrarDetalle();
        this.cargarMantenimientos();
        this.cargarProximos();
      },
      error: (err) => console.error(err)
    });
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este mantenimiento?')) return;
    this.mantenimientoService.eliminar(id).subscribe({
      next: () => this.cargarMantenimientos(),
      error: (err) => console.error(err)
    });
  }

  getBadgeEstado(estado: string): string {
    switch (estado) {
      case 'EnTaller':   return 'badge-taller';
      case 'Finalizado': return 'badge-finalizado';
      case 'Cancelado':  return 'badge-cancelado';
      default:           return 'badge-taller';
    }
  }

  getLabelEstado(estado: string): string {
    switch (estado) {
      case 'EnTaller':   return '🔧 En taller';
      case 'Finalizado': return '✅ Finalizado';
      case 'Cancelado':  return '❌ Cancelado';
      default:           return estado;
    }
  }

  cerrarModal() { this.mostrarModal = false; }

  cerrarDetalle() {
    this.mostrarDetalle   = false;
    this.seleccionado     = null;
    this.mostrarFinalizar = false;
  }

  // =========================
  // PDF INDIVIDUAL
  // =========================

  async descargarPDF(m: any) {
    await this.pdfService.generarPDFMantenimiento(m);
  }

  // =========================
  // EXPORTAR EXCEL
  // =========================

  exportarExcel() {
    const datos = this.mantenimientosFiltrados.map(m => ({
      'ID':                  m.id,
      'Vehículo':            m.vehiculo?.placa      ?? '-',
      'Marca/Modelo':        `${m.vehiculo?.marca ?? ''} ${m.vehiculo?.modelo ?? ''}`.trim(),
      'Tipo':                m.tipoMantenimiento,
      'Taller':              m.nombreTaller,
      'Técnico':             m.tecnicoResponsable   || '-',
      'Teléfono taller':     m.telefonoTaller       || '-',
      'Fecha entrada':       new Date(m.fechaEntrada).toLocaleString(),
      'Fecha salida':        m.fechaSalida ? new Date(m.fechaSalida).toLocaleString() : '-',
      'Kilometraje entrada': m.kilometrajeEntrada,
      'Próximo km':          m.kilometrajeSiguiente || '-',
      'Próxima fecha':       m.fechaSiguiente ? new Date(m.fechaSiguiente).toLocaleDateString() : '-',
      'Trabajos':            m.trabajosRealizados,
      'Repuestos':           m.repuestosUtilizados  || '-',
      'Costo mano obra':     m.costoManoObra,
      'Costo repuestos':     m.costoRepuestos,
      'Costo total':         m.costoTotal,
      'Estado':              m.estado,
      'Observaciones':       m.observaciones        || '-',
    }));

    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6  }, { wch: 12 }, { wch: 20 }, { wch: 22 },
      { wch: 25 }, { wch: 20 }, { wch: 15 }, { wch: 20 },
      { wch: 20 }, { wch: 15 }, { wch: 12 }, { wch: 15 },
      { wch: 40 }, { wch: 30 }, { wch: 15 }, { wch: 15 },
      { wch: 12 }, { wch: 12 }, { wch: 30 }
    ];

    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Mantenimiento');
    XLSX.writeFile(libro, `mantenimiento_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  // =========================
  // EXPORTAR PDF GENERAL
  // =========================

  exportarPDF() {
    const doc   = new jsPDF('landscape');
    const VERDE = [21, 128, 61] as [number, number, number];

    doc.setFillColor(...VERDE);
    doc.rect(0, 0, 297, 20, 'F');
    doc.setFontSize(14);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text('REPORTE DE TALLER Y MANTENIMIENTO', 148, 13, { align: 'center' });

    doc.setFontSize(9);
    doc.setTextColor(80, 80, 80);
    doc.setFont('helvetica', 'normal');
    doc.text(`Generado: ${new Date().toLocaleString()}`, 14, 28);
    doc.text(`Total registros: ${this.mantenimientosFiltrados.length}`, 14, 35);
    doc.text(`En taller: ${this.enTaller}`, 80, 35);
    doc.text(`Costo total: $${this.costoTotal.toLocaleString()}`, 140, 35);

    autoTable(doc, {
      startY: 42,
      head: [[
        '#', 'Vehículo', 'Tipo', 'Taller',
        'Fecha entrada', 'Km entrada',
        'Costo total', 'Estado'
      ]],
      body: this.mantenimientosFiltrados.map(m => [
        m.id,
        m.vehiculo?.placa   ?? '-',
        m.tipoMantenimiento,
        m.nombreTaller,
        new Date(m.fechaEntrada).toLocaleDateString(),
        m.kilometrajeEntrada,
        `$${(m.costoTotal || 0).toLocaleString()}`,
        m.estado
      ]),
      headStyles: { fillColor: VERDE, textColor: [255,255,255], fontStyle: 'bold', fontSize: 8 },
      bodyStyles: { fontSize: 8 },
      alternateRowStyles: { fillColor: [245, 245, 245] },
      columnStyles: {
        0: { cellWidth: 10, halign: 'center' },
        1: { cellWidth: 20 },
        2: { cellWidth: 30 },
        3: { cellWidth: 35 },
        4: { cellWidth: 25 },
        5: { cellWidth: 20, halign: 'right' },
        6: { cellWidth: 25, halign: 'right' },
        7: { cellWidth: 22, halign: 'center' },
      }
    });

    const numPaginas = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= numPaginas; i++) {
      doc.setPage(i);
      doc.setDrawColor(...VERDE);
      doc.setLineWidth(0.3);
      doc.line(14, 200, 283, 200);
      doc.setFontSize(7);
      doc.setTextColor(120, 120, 120);
      doc.text('Sistema de Gestión de Flota', 148, 204, { align: 'center' });
      doc.text(`Página ${i} de ${numPaginas}`, 283, 204, { align: 'right' });
    }

    doc.save(`mantenimiento_${new Date().toISOString().slice(0,10)}.pdf`);
  }

}