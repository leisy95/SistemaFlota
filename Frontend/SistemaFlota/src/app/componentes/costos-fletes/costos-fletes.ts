import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-costos-fletes',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './costos-fletes.html',
  styleUrls: ['./costos-fletes.scss']
})
export class CostosFleteComponent implements OnInit {
  vista: 'lista' | 'nuevo' | 'ver' | 'editar' | 'verificar' = 'lista';
  registros: any[] = [];
  autorizaciones: any[] = [];
  registroSeleccionado: any = null;

  filtroDesde = '';
  filtroHasta = '';
  filtroConductor = '';
  filtroEstado = '';
  filtroCiudad = '';

  form = {
    autorizacionId: null as number | null,
    peajes: 0, combustible: 0, parqueos: 0,
    descarguesMcia: 0, cargueMateriales: 0,
    alimentacion: 0, hospedaje: 0, varios: 0,
    observaciones: ''
  };

  autorizacionSeleccionada: any = null;
  firmaDataUrl: string | null = null;
  nombreVerificador = '';

  get usuario(): string {
    const u = JSON.parse(sessionStorage.getItem('user') || '{}');
    return u.username ?? '';
  }

  get rol(): string {
    const u = JSON.parse(sessionStorage.getItem('user') || '{}');
    return u.rol ?? '';
  }

  get puedeVerificar(): boolean {
    return ['Admin', 'Jefe', 'Facturacion'].includes(this.rol);
  }

  get total(): number {
    return (this.form.peajes || 0) + (this.form.combustible || 0) +
      (this.form.parqueos || 0) + (this.form.descarguesMcia || 0) +
      (this.form.cargueMateriales || 0) + (this.form.alimentacion || 0) +
      (this.form.hospedaje || 0) + (this.form.varios || 0);
  }

  get totalGastosFiltrados(): number {
    return this.registros.reduce((s, r) => s + (r.total || 0), 0);
  }

  get totalKilosFiltrados(): number {
    return this.registros.reduce((s, r) => s + (r.autorizacion?.kilosTransportados || 0), 0);
  }

  get promedioPorKilo(): number {
    if (this.totalKilosFiltrados === 0) return 0;
    return this.totalGastosFiltrados / this.totalKilosFiltrados;
  }

  get pendientes(): number { return this.registros.filter(r => r.estado === 'Pendiente').length; }
  get verificados(): number { return this.registros.filter(r => r.estado === 'Verificado').length; }

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: 'Bearer ' + sessionStorage.getItem('token') });
  }

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.cargar();
    this.cargarAutorizaciones();
  }

  cargar() {
    let url = `${environment.apiUrl}/CostosFletes?`;
    if (this.filtroDesde) url += `desde=${this.filtroDesde}&`;
    if (this.filtroHasta) url += `hasta=${this.filtroHasta}&`;
    if (this.filtroConductor) url += `conductor=${this.filtroConductor}&`;
    if (this.filtroEstado) url += `estado=${this.filtroEstado}&`;
    if (this.filtroCiudad) url += `ciudad=${this.filtroCiudad}&`;
    this.http.get<any[]>(url, { headers: this.headers }).subscribe({
      next: d => this.registros = d,
      error: e => console.error(e)
    });
  }

  cargarAutorizaciones() {
    this.http.get<any[]>(`${environment.apiUrl}/Autorizaciones`, { headers: this.headers }).subscribe({
      next: d => this.autorizaciones = d,
      error: e => console.error(e)
    });
  }

  seleccionarAutorizacion() {
    this.autorizacionSeleccionada = this.autorizaciones.find(a => a.id === this.form.autorizacionId) || null;
  }

  resetForm() {
    this.form = {
      autorizacionId: null, peajes: 0, combustible: 0, parqueos: 0,
      descarguesMcia: 0, cargueMateriales: 0, alimentacion: 0,
      hospedaje: 0, varios: 0, observaciones: ''
    };
    this.autorizacionSeleccionada = null;
  }

  nuevo() { this.resetForm(); this.vista = 'nuevo'; }

  guardar() {
    if (!this.form.autorizacionId) { alert('Seleccione una autorización'); return; }
    const body = { ...this.form, total: this.total };
    this.http.post(`${environment.apiUrl}/CostosFletes`, body, { headers: this.headers }).subscribe({
      next: () => { this.vista = 'lista'; this.cargar(); },
      error: e => { console.error(e); alert('Error guardando'); }
    });
  }

  ver(r: any) { this.registroSeleccionado = r; this.vista = 'ver'; }

  editar(r: any) {
    this.registroSeleccionado = r;
    this.form = {
      autorizacionId: r.autorizacionId,
      peajes: r.peajes, combustible: r.combustible,
      parqueos: r.parqueos, descarguesMcia: r.descarguesMcia,
      cargueMateriales: r.cargueMateriales, alimentacion: r.alimentacion,
      hospedaje: r.hospedaje, varios: r.varios,
      observaciones: r.observaciones ?? ''
    };
    this.autorizacionSeleccionada = r.autorizacion;
    this.vista = 'editar';
  }

  guardarEdicion() {
    const body = { ...this.form, total: this.total };
    this.http.put(`${environment.apiUrl}/CostosFletes/${this.registroSeleccionado.id}`, body, { headers: this.headers }).subscribe({
      next: () => { this.vista = 'lista'; this.cargar(); },
      error: e => { console.error(e); alert('Error editando'); }
    });
  }

  abrirVerificar(r: any) {
    this.registroSeleccionado = r;
    this.firmaDataUrl = null;
    this.nombreVerificador = this.usuario;
    this.vista = 'verificar';
    setTimeout(() => this.iniciarCanvas(), 100);
  }

  iniciarCanvas() {
    const canvas = document.getElementById('firmaVerifica') as HTMLCanvasElement;
    if (!canvas) return;
    const ctx = canvas.getContext('2d')!;
    let dibujando = false;
    canvas.addEventListener('mousedown', e => { dibujando = true; ctx.beginPath(); ctx.moveTo(e.offsetX, e.offsetY); });
    canvas.addEventListener('mousemove', e => { if (!dibujando) return; ctx.lineTo(e.offsetX, e.offsetY); ctx.stroke(); });
    canvas.addEventListener('mouseup', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
    canvas.addEventListener('touchstart', e => { e.preventDefault(); dibujando = true; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.beginPath(); ctx.moveTo(t.clientX-r.left, t.clientY-r.top); }, {passive:false});
    canvas.addEventListener('touchmove', e => { e.preventDefault(); if (!dibujando) return; const t = e.touches[0]; const r = canvas.getBoundingClientRect(); ctx.lineTo(t.clientX-r.left, t.clientY-r.top); ctx.stroke(); }, {passive:false});
    canvas.addEventListener('touchend', () => { dibujando = false; this.firmaDataUrl = canvas.toDataURL(); });
  }

  limpiarFirma() {
    const canvas = document.getElementById('firmaVerifica') as HTMLCanvasElement;
    if (canvas) canvas.getContext('2d')!.clearRect(0, 0, canvas.width, canvas.height);
    this.firmaDataUrl = null;
  }

  confirmarVerificacion() {
    if (!this.firmaDataUrl) { alert('Dibuje la firma'); return; }
    const body = { verificadoPor: this.nombreVerificador, firmaVerificacion: this.firmaDataUrl };
    this.http.put(`${environment.apiUrl}/CostosFletes/${this.registroSeleccionado.id}/verificar`, body, { headers: this.headers }).subscribe({
      next: () => { this.vista = 'lista'; this.cargar(); },
      error: e => { console.error(e); alert('Error verificando'); }
    });
  }

  exportarPDF(r: any) {
    const doc = new jsPDF('p', 'mm', 'letter');
    const VERDE: [number,number,number] = [26,127,90];
    const W = 216; const M = 10;
    doc.setFillColor(...VERDE); doc.rect(M, M, W-M*2, 14, 'F');
    doc.setTextColor(255,255,255); doc.setFont('helvetica','bold'); doc.setFontSize(12);
    doc.text('RELACION DE GASTOS DE VIAJE', W/2, M+9, {align:'center'});
    doc.setTextColor(0,0,0); doc.setFont('helvetica','normal'); doc.setFontSize(8);
    let y = M+20;
    const aut = r.autorizacion;
    autoTable(doc, {
      startY: y,
      head: [['Campo','Valor']],
      body: [
        ['Conductor', aut?.conductor ?? ''],
        ['Placa', aut?.placa ?? ''],
        ['Ciudad destino', aut?.ciudadDestino ?? ''],
        ['Fecha', new Date(r.fechaRegistro).toLocaleDateString('es-CO')],
        ['Kilos transportados', (aut?.kilosTransportados ?? 0).toLocaleString('es-CO') + ' kg'],
        ['Clientes entregados', aut?.clientesEntregados ?? ''],
      ],
      headStyles: {fillColor: VERDE},
      margin: {left: M, right: M}
    });
    y = (doc as any).lastAutoTable.finalY + 5;
    autoTable(doc, {
      startY: y,
      head: [['Concepto','Valor']],
      body: [
        ['Peajes', '$' + (r.peajes||0).toLocaleString('es-CO')],
        ['Combustible', '$' + (r.combustible||0).toLocaleString('es-CO')],
        ['Parqueos', '$' + (r.parqueos||0).toLocaleString('es-CO')],
        ['Descargue Mcia', '$' + (r.descarguesMcia||0).toLocaleString('es-CO')],
        ['Cargue materiales', '$' + (r.cargueMateriales||0).toLocaleString('es-CO')],
        ['Alimentación', '$' + (r.alimentacion||0).toLocaleString('es-CO')],
        ['Hospedaje', '$' + (r.hospedaje||0).toLocaleString('es-CO')],
        ['Varios', '$' + (r.varios||0).toLocaleString('es-CO')],
        ['TOTAL', '$' + (r.total||0).toLocaleString('es-CO')],
      ],
      headStyles: {fillColor: VERDE},
      bodyStyles: {fontSize: 9},
      didParseCell: (data) => {
        if (data.row.index === 8) {
          data.cell.styles.fillColor = [232,245,233];
          data.cell.styles.fontStyle = 'bold';
        }
      },
      margin: {left: M, right: M}
    });
    if (r.observaciones) {
      y = (doc as any).lastAutoTable.finalY + 5;
      doc.setFontSize(8); doc.text('Observaciones: ' + r.observaciones, M, y);
    }
    if (r.firmaVerificacion) {
      y = (doc as any).lastAutoTable.finalY + 15;
      try { doc.addImage(r.firmaVerificacion, 'PNG', M, y, 60, 15); } catch(e) {}
      doc.line(M, y+18, M+70, y+18);
      doc.text('Verificado por: ' + (r.verificadoPor ?? ''), M, y+22);
    }
    doc.setFont('helvetica','bold'); doc.setFontSize(10);
    doc.text('FIN DEL DOCUMENTO', W/2, 270, {align:'center'});
    doc.save('Gastos_Viaje_'+r.id+'.pdf');
  }

  exportarExcel() {
    const data = this.registros.map(r => ({
      'Autorización': r.autorizacionId,
      'Conductor': r.autorizacion?.conductor ?? '',
      'Placa': r.autorizacion?.placa ?? '',
      'Destino': r.autorizacion?.ciudadDestino ?? '',
      'Fecha': new Date(r.fechaRegistro).toLocaleDateString('es-CO'),
      'Kilos': r.autorizacion?.kilosTransportados ?? 0,
      'Peajes': r.peajes,
      'Combustible': r.combustible,
      'Parqueos': r.parqueos,
      'Descargue Mcia': r.descarguesMcia,
      'Cargue Materiales': r.cargueMateriales,
      'Alimentación': r.alimentacion,
      'Hospedaje': r.hospedaje,
      'Varios': r.varios,
      'Total': r.total,
      'Estado': r.estado,
      'Verificado por': r.verificadoPor ?? ''
    }));
    const ws = XLSX.utils.json_to_sheet(data);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Costos Fletes');
    XLSX.writeFile(wb, 'CostosFletes_'+new Date().toISOString().slice(0,10)+'.xlsx');
  }
}