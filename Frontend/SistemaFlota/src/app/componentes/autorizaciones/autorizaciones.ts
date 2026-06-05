import {
  Component, OnInit, AfterViewInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil, timeout, catchError } from 'rxjs/operators';
import { TimeoutError, throwError } from 'rxjs';

import { ConductoresService }    from '../../services/conductores.service';
import { VehiculosService }      from '../../services/vehiculos.service';
import { AutorizacionesService } from '../../services/autorizaciones.service';
import { ConfiguracionService }  from '../../services/configuracion.service';
import { PdfService }            from '../../services/pdf.service';
import * as XLSX from 'xlsx';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-autorizaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './autorizaciones.html',
  styleUrls: ['./autorizaciones.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutorizacionesComponent implements OnInit, AfterViewInit, OnDestroy {

  private destroy$ = new Subject<void>();

  pasoActual: number = 1;
  conductores:           any[] = [];
  vehiculos:             any[] = [];
  conductorSeleccionado: any   = null;
  autorizacionActual:    any   = null;

  form = {
    vehiculoId: 0, destinoCompleto: '', cantidadClientes: 0,
    pesoKilos: 0, tipoVuelta: '', descripcionCarga: '', numeroGuia: ''
  };

  // ✅ Facturas con peso individual
  facturasClientes: { facturaRemision: string; cliente: string; pesoKilos: number | null }[] = [];

  guiaGenerada     = '';
  usuarioFirma     = '';
  observacionFirma = '';
  notificacion:    string | null = null;
  autorizaciones:  any[] = [];
  autorizacionesFiltradas: any[] = [];

  vistaLista    = true;
  rolUsuario    = '';
  nombreEmpresa = 'la empresa';

  mostrarModalLlegada    = false;
  mostrarModalConfirmar  = false;
  autorizacionLlegada:   any = null;

  formLlegada = {
    kilometrajeFinal: null as number | null,
    novedadesViaje:   '',
    estadoVehiculo:   'Bueno'
  };

  readonly estadosVehiculo = ['Bueno', 'Novedad', 'Requiere taller'];

  filtroBusqueda   = '';
  filtroEstado     = '';
  filtroTipo       = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  readonly tiposVuelta = [
    { value: 'Solo entrega',       label: 'Solo entrega' },
    { value: 'Mixta',              label: 'Mixta' },
    { value: 'Recogida y entrega', label: 'Recogida y entrega' },
    { value: 'Solo recoge',        label: 'Solo recoge' },
    { value: 'Mensajería',         label: 'Mensajería' },
  ];

  constructor(
    private conductoresService:    ConductoresService,
    private vehiculosService:      VehiculosService,
    private autorizacionesService: AutorizacionesService,
    private configuracionService:  ConfiguracionService,
    private pdfService:            PdfService,
    private cdr:                   ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (raw) this.rolUsuario = JSON.parse(raw).rol;
    this.obtenerConductores();
    this.obtenerVehiculos();
    this.obtenerAutorizaciones();
    this.configuracionService.obtenerConfiguracion().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: any) => { if (data.nombreEmpresa?.trim()) this.nombreEmpresa = data.nombreEmpresa; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  ngAfterViewInit(): void {}
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  aplicarFiltros(): void {
    const q = this.filtroBusqueda.toLowerCase();
    let base: any[];
    switch (this.rolUsuario) {
      case 'Facturacion': base = [...this.autorizaciones]; break;
      case 'Bodega':      base = this.autorizaciones.filter(a => a.estado === 'Bodega'); break;
      case 'Porteria':    base = this.autorizaciones.filter(a => a.estado === 'Porteria' || (a.estado === 'Autorizado' && a.estadoLlegada === 'ReportadaLlegada')); break;
      default:            base = [...this.autorizaciones];
    }
    this.autorizacionesFiltradas = base.filter(a => {
      const okEstado = !this.filtroEstado || a.estado === this.filtroEstado;
      const okTipo   = !this.filtroTipo   || a.tipoVuelta === this.filtroTipo;
      const fecha    = new Date(a.fechaCreacion);
      const okDesde  = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta  = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq   = !q ||
        a.conductor?.nombre?.toLowerCase().includes(q) ||
        a.vehiculo?.placa?.toLowerCase().includes(q)   ||
        a.destinoCompleto?.toLowerCase().includes(q)   ||
        a.numeroGuia?.toLowerCase().includes(q);
      return okEstado && okTipo && okDesde && okHasta && okBusq;
    });
    this.cdr.markForCheck();
  }

  limpiarFiltros(): void {
    this.filtroBusqueda = ''; this.filtroEstado = '';
    this.filtroTipo = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  private actualizarFiltradas(): void { this.aplicarFiltros(); }

  get puedeCrear(): boolean { return !['Bodega', 'Porteria'].includes(this.rolUsuario); }

  get tituloPorRol(): string {
    switch (this.rolUsuario) {
      case 'Facturacion': return 'Autorizaciones de Salida';
      case 'Bodega':      return 'Pendientes de Bodega';
      case 'Porteria':    return 'Control de Portería';
      default:            return 'Autorizaciones de Salida';
    }
  }

  get esMensajeria(): boolean { return this.form.tipoVuelta === 'Mensajería'; }
  trackById(_i: number, item: any): number { return item.id; }
  trackByIdx(i: number, _item: any): number { return i; }

  // ✅ Peso total de todas las facturas
  get pesoTotalFacturas(): number {
    return this.facturasClientes.reduce((sum, f) => sum + (f.pesoKilos || 0), 0);
  }

  // ✅ Recalcular peso en kilos al cambiar facturas
  recalcularPeso(): void {
    const total = this.pesoTotalFacturas;
    if (total > 0) this.form.pesoKilos = total;
    this.cdr.markForCheck();
  }

  obtenerConductores() {
    this.conductoresService.obtenerConductores().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.conductores = data; this.cdr.markForCheck(); },
      error: (err) => console.error(err)
    });
  }

  obtenerVehiculos() {
    this.vehiculosService.obtenerVehiculos().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.vehiculos = data; this.cdr.markForCheck(); },
      error: (err) => console.error(err)
    });
  }

  obtenerAutorizaciones() {
    this.autorizacionesService.obtenerAutorizaciones().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.autorizaciones = data; this.actualizarFiltradas(); },
      error: (err) => console.error(err)
    });
  }

  abrirReporteLlegada(autorizacion: any) {
    this.autorizacionLlegada = autorizacion;
    this.formLlegada = { kilometrajeFinal: null, novedadesViaje: '', estadoVehiculo: 'Bueno' };
    this.mostrarModalLlegada = true;
    this.cdr.markForCheck();
  }

  guardarReporteLlegada() {
    if (!this.formLlegada.kilometrajeFinal) { alert('Ingrese el kilometraje final'); return; }
    this.autorizacionesService.reportarLlegada(this.autorizacionLlegada.id, {
      kilometrajeFinal: this.formLlegada.kilometrajeFinal,
      novedadesViaje:   this.formLlegada.novedadesViaje,
      estadoVehiculo:   this.formLlegada.estadoVehiculo
    }).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: () => {
        this.mostrarModalLlegada = false;
        this.mostrarNotificacion(`✅ Llegada reportada — WhatsApp enviado automáticamente`);
        this.obtenerAutorizaciones();
        this.cdr.markForCheck();
      }, error: () => {}
    });
  }

  abrirConfirmarLlegada(autorizacion: any) {
    this.autorizacionLlegada = autorizacion;
    this.usuarioFirma = ''; this.observacionFirma = '';
    this.mostrarModalConfirmar = true;
    this.cdr.markForCheck();
  }

  guardarConfirmarLlegada() {
    if (!this.usuarioFirma) { alert('Ingrese nombre del portero'); return; }
    this.autorizacionesService.confirmarLlegada(this.autorizacionLlegada.id, {
      firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma
    }).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: () => {
        this.mostrarModalConfirmar = false;
        this.mostrarNotificacion(`✅ Llegada confirmada por portería`);
        this.obtenerAutorizaciones();
        this.cdr.markForCheck();
      }, error: () => {}
    });
  }

  seleccionarConductor(conductor: any) {
    this.conductorSeleccionado = conductor; this.pasoActual = 2; this.vistaLista = false; this.cdr.markForCheck();
  }

  generarGuia() {
    this.autorizacionesService.generarGuia().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: any) => { this.form.numeroGuia = data.guia; this.guiaGenerada = data.guia; this.cdr.markForCheck(); },
      error: (err) => console.error(err)
    });
  }

  agregarFactura() {
    this.facturasClientes = [...this.facturasClientes, { facturaRemision: '', cliente: '', pesoKilos: null }];
    this.cdr.markForCheck();
  }

  eliminarFactura(i: number) {
    this.facturasClientes = this.facturasClientes.filter((_, idx) => idx !== i);
    this.recalcularPeso();
    this.cdr.markForCheck();
  }

  obtenerFacturas(json: string): any[] {
    if (!json) return [];
    try { return JSON.parse(json); } catch { return []; }
  }

  guardarDetalles() {
    if (!this.form.vehiculoId) { alert('Seleccione una placa'); return; }
    if (!this.form.tipoVuelta) { alert('Seleccione tipo de vuelta'); return; }
    if (!this.esMensajeria) {
      if (!this.form.destinoCompleto)  { alert('Ingrese el destino'); return; }
      if (!this.form.cantidadClientes) { alert('Ingrese cantidad de clientes'); return; }
      if (!this.form.pesoKilos)        { alert('Ingrese el peso'); return; }
      if (!this.form.descripcionCarga) { alert('Ingrese descripción de la carga'); return; }
    }
    const datos = {
      conductorId:      Number(this.conductorSeleccionado.id),
      vehiculoId:       Number(this.form.vehiculoId),
      destinoCompleto:  this.form.destinoCompleto  || '',
      cantidadClientes: Number(this.form.cantidadClientes) || 0,
      pesoKilos:        Number(this.form.pesoKilos) || 0,
      tipoVuelta:       this.form.tipoVuelta,
      descripcionCarga: this.form.descripcionCarga || '',
      numeroGuia:       this.form.numeroGuia       || '',
      facturasClientes: this.facturasClientes.length > 0 ? JSON.stringify(this.facturasClientes) : null
    };
    this.autorizacionesService.crear(datos).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(err instanceof TimeoutError ? 'Timeout' : `Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: (data) => { this.autorizacionActual = data; this.pasoActual = 3; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  firmarFacturacion() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }
    this.autorizacionesService.firmarFacturacion(this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: (data) => { this.autorizacionActual = data; this.usuarioFirma = ''; this.observacionFirma = ''; this.pasoActual = 4; this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  firmarBodega() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }
    this.autorizacionesService.firmarBodega(this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: (data) => { this.autorizacionActual = data; this.usuarioFirma = ''; this.observacionFirma = ''; this.pasoActual = 5; this.mostrarNotificacion(`Salida autorizada por Bodega`); this.cdr.markForCheck(); },
      error: () => {}
    });
  }

  firmarPorteria() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }
    this.autorizacionesService.firmarPorteria(this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: (data) => {
        this.autorizacionActual = data;
        this.mostrarNotificacion(`✅ ${this.conductorSeleccionado.nombre} — ¡Buen viaje! WhatsApp enviado automáticamente`);
        this.obtenerAutorizaciones();
        this.cdr.markForCheck();
        setTimeout(() => this.resetear(), 3000);
      }, error: () => {}
    });
  }

  continuarAutorizacion(autorizacion: any) {
    this.autorizacionActual    = autorizacion;
    this.conductorSeleccionado = autorizacion.conductor;
    this.vistaLista            = false;
    switch (this.rolUsuario) {
      case 'Facturacion': this.pasoActual = 3; break;
      case 'Bodega':      this.pasoActual = 4; break;
      case 'Porteria':    this.pasoActual = 5; break;
      default:
        switch (autorizacion.estado) {
          case 'Pendiente': this.pasoActual = 3; break;
          case 'Bodega':    this.pasoActual = 4; break;
          case 'Porteria':  this.pasoActual = 5; break;
          default:          this.pasoActual = 1;
        }
    }
    this.cdr.markForCheck();
  }
  
setPaso(n: number): void { this.pasoActual = n; this.cdr.markForCheck(); }

  resetear() {
    this.pasoActual = 1; this.conductorSeleccionado = null; this.autorizacionActual = null; this.vistaLista = true;
    this.facturasClientes = []; this.guiaGenerada = '';
    this.form = { vehiculoId: 0, destinoCompleto: '', cantidadClientes: 0, pesoKilos: 0, tipoVuelta: '', descripcionCarga: '', numeroGuia: '' };
    this.usuarioFirma = ''; this.observacionFirma = '';
    this.obtenerAutorizaciones(); this.cdr.markForCheck();
  }

  mostrarNotificacion(mensaje: string) {
    this.notificacion = mensaje; this.cdr.markForCheck();
    setTimeout(() => { this.notificacion = null; this.cdr.markForCheck(); }, 5000);
  }

  getBadgeClass(estado: string): string {
    switch (estado) {
      case 'Pendiente':  return 'badge-pendiente';
      case 'Bodega':     return 'badge-bodega';
      case 'Porteria':   return 'badge-porteria';
      case 'Autorizado': return 'badge-autorizado';
      case 'Rechazado':  return 'badge-rechazado';
      default:           return 'badge-pendiente';
    }
  }

  getLlegadaBadge(a: any): string {
    if (!a.estadoLlegada) return '';
    if (a.estadoLlegada === 'ReportadaLlegada') return 'badge-llegada-reportada';
    if (a.estadoLlegada === 'Completada')       return 'badge-llegada-completa';
    return '';
  }

  async descargarPDF(autorizacion: any) { await this.pdfService.generarPDFAutorizacion(autorizacion); }

  exportarExcel() {
    const datos = this.autorizacionesFiltradas.map(a => ({
      'ID': a.id, 'Fecha': new Date(a.fechaCreacion).toLocaleString(),
      'Conductor': a.conductor?.nombre ?? '-', 'Vehículo': a.vehiculo?.placa ?? '-',
      'Tipo': a.tipoVuelta ?? '-', 'Destino': a.destinoCompleto || 'Mensajería',
      'Estado': a.estado, 'Estado Llegada': a.estadoLlegada ?? 'En ruta',
      'Km Final': a.kilometrajeFinal ?? '-', 'Novedades': a.novedadesViaje || '-',
      'Estado Vehículo': a.estadoVehiculoLlegada ?? '-',
      'Facturación': a.usuarioFacturacion || '-', 'Bodega': a.usuarioBodega || '-', 'Portería': a.usuarioPorteria || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Autorizaciones');
    XLSX.writeFile(libro, `autorizaciones_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  exportarPDF() {
    const doc = new jsPDF('landscape');
    const VERDE = [21,128,61] as [number,number,number];
    doc.setFillColor(...VERDE); doc.rect(0,0,297,20,'F');
    doc.setFontSize(14); doc.setTextColor(255,255,255); doc.setFont('helvetica','bold');
    doc.text('REPORTE DE AUTORIZACIONES', 148, 13, { align: 'center' });
    autoTable(doc, {
      startY: 30,
      head: [['#','Conductor','Vehículo','Tipo','Destino','Estado','Llegada','Km Final']],
      body: this.autorizacionesFiltradas.map(a => [
        a.id, a.conductor?.nombre??'-', a.vehiculo?.placa??'-',
        a.tipoVuelta??'-', a.destinoCompleto||'Mensajería',
        a.estado, a.estadoLlegada??'En ruta', a.kilometrajeFinal??'-'
      ]),
      headStyles: { fillColor: VERDE, textColor: [255,255,255], fontStyle: 'bold', fontSize: 8 },
      bodyStyles: { fontSize: 8 }, alternateRowStyles: { fillColor: [245,245,245] }
    });
    doc.save(`autorizaciones_${new Date().toISOString().slice(0,10)}.pdf`);
  }
}