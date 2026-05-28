import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil, timeout, catchError } from 'rxjs/operators';
import { TimeoutError, throwError } from 'rxjs';

import { ConductoresService }    from '../../services/conductores.service';
import { VehiculosService }      from '../../services/vehiculos.service';
import { AutorizacionesService } from '../../services/autorizaciones.service';
import { ContactosService }      from '../../services/contactos.service';
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

  pasoActual = 1;
  conductores: any[] = [];
  vehiculos:   any[] = [];
  conductorSeleccionado: any = null;
  autorizacionActual:    any = null;

  form = {
    vehiculoId:       0,
    destinoCompleto:  '',
    cantidadClientes: 0,
    pesoKilos:        0,
    tipoVuelta:       '',
    descripcionCarga: '',
    numeroGuia:       ''
  };

  facturasClientes: { facturaRemision: string; cliente: string }[] = [];
  guiaGenerada  = '';
  usuarioFirma  = '';
  observacionFirma = '';
  notificacion: string | null = null;
  autorizaciones: any[] = [];

  // ── Cache del getter para evitar re-renders infinitos ────────────────────────
  autorizacionesFiltradas: any[] = [];

  vistaLista    = true;
  rolUsuario    = '';
  nombreEmpresa = 'la empresa';

  readonly tiposVuelta = [
    { value: 'Solo entrega',       label: '📦 Solo entrega' },
    { value: 'Mixta',              label: '🔄 Mixta' },
    { value: 'Recogida y entrega', label: '↩️ Recogida y entrega' },
    { value: 'Solo recoge',        label: '📥 Solo recoge' },
    { value: 'Mensajería',         label: '✉️ Mensajería' },
  ];

  constructor(
    private conductoresService:    ConductoresService,
    private vehiculosService:      VehiculosService,
    private autorizacionesService: AutorizacionesService,
    private contactosService:      ContactosService,
    private configuracionService:  ConfiguracionService,
    private pdfService:            PdfService,
    private cdr:                   ChangeDetectorRef
  ) {}

  // ── Lifecycle ────────────────────────────────────────────────────────────────

  ngOnInit(): void {
    const user = localStorage.getItem('user');
    if (user) {
      const userData = JSON.parse(user);
      this.rolUsuario = userData.rol;
    }
    this.obtenerConductores();
    this.obtenerVehiculos();
    this.obtenerAutorizaciones();

    this.configuracionService.obtenerConfiguracion()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => {
          if (data.nombreEmpresa && data.nombreEmpresa.trim() !== '')
            this.nombreEmpresa = data.nombreEmpresa;
          this.cdr.markForCheck();
        },
        error: () => {}
      });
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Filtro como método (no getter) para evitar re-renders infinitos ───────────
  private actualizarFiltradas(): void {
    switch (this.rolUsuario) {
      case 'Facturacion':
        this.autorizacionesFiltradas = this.autorizaciones.filter(a => a.estado === 'Pendiente');
        break;
      case 'Bodega':
        this.autorizacionesFiltradas = this.autorizaciones.filter(a => a.estado === 'Bodega');
        break;
      case 'Porteria':
        this.autorizacionesFiltradas = this.autorizaciones.filter(a => a.estado === 'Porteria');
        break;
      default:
        this.autorizacionesFiltradas = [...this.autorizaciones];
    }
    this.cdr.markForCheck();
  }

  get puedeCrear(): boolean {
    return !['Facturacion', 'Bodega', 'Porteria'].includes(this.rolUsuario);
  }

  get tituloPorRol(): string {
    switch (this.rolUsuario) {
      case 'Facturacion': return '🧾 Pendientes de Facturación';
      case 'Bodega':      return '📦 Pendientes de Bodega';
      case 'Porteria':    return '🚪 Pendientes de Portería';
      default:            return '✅ Autorizaciones de Salida';
    }
  }

  get esMensajeria(): boolean {
    return this.form.tipoVuelta === 'Mensajería';
  }

  // ── trackBy para *ngFor ───────────────────────────────────────────────────────
  trackById(_index: number, item: any): number { return item.id; }
  trackByIdx(index: number, _item: any): number { return index; }

  // ── Carga de datos ────────────────────────────────────────────────────────────

  obtenerConductores() {
    this.conductoresService.obtenerConductores()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.conductores = data;
          this.cdr.markForCheck();
        },
        error: (err) => console.error('Conductores:', err)
      });
  }

  obtenerVehiculos() {
    this.vehiculosService.obtenerVehiculos()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.vehiculos = data;
          this.cdr.markForCheck();
        },
        error: (err) => console.error('Vehículos:', err)
      });
  }

  obtenerAutorizaciones() {
    this.autorizacionesService.obtenerAutorizaciones()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.autorizaciones = data;
          this.actualizarFiltradas();
        },
        error: (err) => console.error('Autorizaciones:', err)
      });
  }

  // ── Flujo de pasos ────────────────────────────────────────────────────────────

  seleccionarConductor(conductor: any) {
    this.conductorSeleccionado = conductor;
    this.pasoActual = 2;
    this.vistaLista = false;
    this.cdr.markForCheck();
  }

  generarGuia() {
    this.autorizacionesService.generarGuia()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data: any) => {
          this.form.numeroGuia = data.guia;
          this.guiaGenerada    = data.guia;
          this.cdr.markForCheck();
        },
        error: (err) => console.error('Generar guía:', err)
      });
  }

  agregarFactura() {
    this.facturasClientes = [...this.facturasClientes, { facturaRemision: '', cliente: '' }];
    this.cdr.markForCheck();
  }

  eliminarFactura(index: number) {
    this.facturasClientes = this.facturasClientes.filter((_, i) => i !== index);
    this.cdr.markForCheck();
  }

  obtenerFacturas(json: string): any[] {
    if (!json) return [];
    try { return JSON.parse(json); }
    catch { return []; }
  }

  // ── PASO 2 → Guardar detalles ─────────────────────────────────────────────────

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
      pesoKilos:        Number(this.form.pesoKilos)        || 0,
      tipoVuelta:       this.form.tipoVuelta,
      descripcionCarga: this.form.descripcionCarga || '',
      numeroGuia:       this.form.numeroGuia       || '',
      facturasClientes: this.facturasClientes.length > 0
        ? JSON.stringify(this.facturasClientes)
        : null
    };

    this.autorizacionesService.crear(datos)
      .pipe(
        timeout(15000),
        takeUntil(this.destroy$),
        catchError(err => {
          if (err instanceof TimeoutError) {
            alert('El servidor no respondió en 15s. Verifica que el backend esté activo.');
          } else {
            const msg = err.error?.message ?? err.error ?? err.message ?? 'Error desconocido';
            alert(`Error ${err.status ?? ''}: ${JSON.stringify(msg)}`);
          }
          return throwError(() => err);
        })
      )
      .subscribe({
        next: (data) => {
          this.autorizacionActual = data;
          this.pasoActual = 3;
          this.cdr.markForCheck();
        },
        error: () => {}
      });
  }

  // ── PASO 3 → Firma Facturación ────────────────────────────────────────────────

  firmarFacturacion() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }

    this.autorizacionesService.firmarFacturacion(
      this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(
      timeout(15000),
      takeUntil(this.destroy$),
      catchError(err => {
        alert(err instanceof TimeoutError
          ? 'Timeout: el backend no respondió'
          : `Error al firmar facturación: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
    ).subscribe({
      next: (data) => {
        this.autorizacionActual = data;
        this.usuarioFirma     = '';
        this.observacionFirma = '';
        this.pasoActual = 4;
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  // ── PASO 4 → Firma Bodega ─────────────────────────────────────────────────────

  firmarBodega() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }

    this.autorizacionesService.firmarBodega(
      this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(
      timeout(15000),
      takeUntil(this.destroy$),
      catchError(err => {
        alert(err instanceof TimeoutError
          ? 'Timeout: el backend no respondió'
          : `Error al firmar bodega: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
    ).subscribe({
      next: (data) => {
        this.autorizacionActual = data;
        this.usuarioFirma     = '';
        this.observacionFirma = '';
        this.pasoActual = 5;
        this.mostrarNotificacion(`✅ ${this.conductorSeleccionado.nombre}, salida autorizada por Bodega.`);
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  // ── PASO 5 → Firma Portería ───────────────────────────────────────────────────

  firmarPorteria() {
    if (!this.usuarioFirma) { alert('Ingrese nombre de quien firma'); return; }

    this.autorizacionesService.firmarPorteria(
      this.autorizacionActual.id,
      { firma: this.usuarioFirma, usuario: this.usuarioFirma, observacion: this.observacionFirma }
    ).pipe(
      timeout(15000),
      takeUntil(this.destroy$),
      catchError(err => {
        alert(err instanceof TimeoutError
          ? 'Timeout: el backend no respondió'
          : `Error al firmar portería: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
    ).subscribe({
      next: (data) => {
        this.autorizacionActual = data;
        this.mostrarNotificacion(`🚚 ${this.conductorSeleccionado.nombre} — ¡Buen viaje!`);
        this.enviarWhatsAppConductor();
        this.enviarWhatsAppGrupo();
        this.obtenerAutorizaciones();
        this.cdr.markForCheck();
        setTimeout(() => this.resetear(), 3000);
      },
      error: () => {}
    });
  }

  // ── Continuar desde lista ─────────────────────────────────────────────────────

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

  resetear() {
    this.pasoActual            = 1;
    this.conductorSeleccionado = null;
    this.autorizacionActual    = null;
    this.vistaLista            = true;
    this.facturasClientes      = [];
    this.guiaGenerada          = '';
    this.form = {
      vehiculoId: 0, destinoCompleto: '', cantidadClientes: 0,
      pesoKilos: 0, tipoVuelta: '', descripcionCarga: '', numeroGuia: ''
    };
    this.usuarioFirma     = '';
    this.observacionFirma = '';
    this.obtenerAutorizaciones();
    this.cdr.markForCheck();
  }

  // ── WhatsApp ──────────────────────────────────────────────────────────────────

  enviarWhatsAppConductor() {
    const telefono = this.conductorSeleccionado?.telefono;
    if (!telefono || telefono.trim() === '') return;
    const mensaje = encodeURIComponent(
      `Hola ${this.conductorSeleccionado.nombre}, tu salida ha sido autorizada ✅. Buen viaje y maneja con precaución. - ${this.nombreEmpresa}`
    );
    window.open(`https://wa.me/${telefono.replace(/\D/g, '')}?text=${mensaje}`, '_blank');
  }

  enviarWhatsAppGrupo() {
    this.contactosService.obtenerContactos()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (contactos: any[]) => {
          const activos  = contactos.filter(c => c.activo && c.recibeIncidentes);
          const vehiculo = this.autorizacionActual?.vehiculo?.placa ?? '-';
          const destino  = this.autorizacionActual?.destinoCompleto || 'Mensajería';
          const tipo     = this.autorizacionActual?.tipoVuelta      ?? '-';
          const mensaje  = encodeURIComponent(
`🚚 *SALIDA AUTORIZADA* ✅
━━━━━━━━━━━━━━━━━━
👤 *Conductor:* ${this.conductorSeleccionado.nombre}
🚗 *Vehículo:* ${vehiculo}
📍 *Destino:* ${destino}
🔄 *Tipo:* ${tipo}
🚪 *Portería:* ${this.usuarioFirma}
📅 *Fecha:* ${new Date().toLocaleString()}
━━━━━━━━━━━━━━━━━━
_${this.nombreEmpresa}_`
          );
          activos.forEach((contacto, index) => {
            setTimeout(() => {
              window.open(`https://wa.me/${contacto.numeroWhatsApp.replace(/\D/g, '')}?text=${mensaje}`, '_blank');
            }, index * 1000);
          });
        },
        error: (err) => console.error('WhatsApp grupo:', err)
      });
  }

  // ── Utilidades ────────────────────────────────────────────────────────────────

  mostrarNotificacion(mensaje: string) {
    this.notificacion = mensaje;
    this.cdr.markForCheck();
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

  // ── Exportaciones ─────────────────────────────────────────────────────────────

  async descargarPDF(autorizacion: any) {
    await this.pdfService.generarPDFAutorizacion(autorizacion);
  }

  exportarExcel() {
    const datos = this.autorizacionesFiltradas.map(a => ({
      'ID':                a.id,
      'Fecha':             new Date(a.fechaCreacion).toLocaleString(),
      'Conductor':         a.conductor?.nombre      ?? '-',
      'Vehículo':          a.vehiculo?.placa        ?? '-',
      'Tipo de vuelta':    a.tipoVuelta             ?? '-',
      'Destino':           a.destinoCompleto        || 'Mensajería',
      'Clientes':          a.cantidadClientes       ?? '-',
      'Peso (kg)':         a.pesoKilos              ?? '-',
      'Guía':              a.numeroGuia             || '-',
      'Facturas':          a.facturasClientes       || '-',
      'Descripción':       a.descripcionCarga       || '-',
      'Estado':            a.estado,
      'Facturación':       a.usuarioFacturacion     || '-',
      'Bodega':            a.usuarioBodega          || '-',
      'Portería':          a.usuarioPorteria        || '-',
      'Obs. Facturación':  a.observacionFacturacion || '-',
      'Obs. Bodega':       a.observacionBodega      || '-',
      'Obs. Portería':     a.observacionPorteria    || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      {wch:6},{wch:20},{wch:22},{wch:12},{wch:18},{wch:30},
      {wch:10},{wch:10},{wch:18},{wch:30},{wch:30},{wch:12},
      {wch:18},{wch:18},{wch:18},{wch:25},{wch:25},{wch:25}
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Autorizaciones');
    XLSX.writeFile(libro, `autorizaciones_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  exportarPDF() {
    const doc   = new jsPDF('landscape');
    const VERDE = [21, 128, 61] as [number, number, number];
    doc.setFillColor(...VERDE);
    doc.rect(0, 0, 297, 20, 'F');
    doc.setFontSize(14);
    doc.setTextColor(255, 255, 255);
    doc.setFont('helvetica', 'bold');
    doc.text('REPORTE DE AUTORIZACIONES DE SALIDA', 148, 13, { align: 'center' });
    doc.setFontSize(9);
    doc.setTextColor(80, 80, 80);
    doc.setFont('helvetica', 'normal');
    doc.text(`Generado: ${new Date().toLocaleString()}`, 14, 28);
    doc.text(`Total registros: ${this.autorizacionesFiltradas.length}`, 14, 35);
    autoTable(doc, {
      startY: 40,
      head: [['#','Fecha','Conductor','Vehículo','Tipo','Destino','Guía','Estado','Facturación','Bodega','Portería']],
      body: this.autorizacionesFiltradas.map(a => [
        a.id,
        new Date(a.fechaCreacion).toLocaleDateString(),
        a.conductor?.nombre  ?? '-',
        a.vehiculo?.placa    ?? '-',
        a.tipoVuelta         ?? '-',
        a.destinoCompleto    || 'Mensajería',
        a.numeroGuia         || '-',
        a.estado,
        a.usuarioFacturacion || '-',
        a.usuarioBodega      || '-',
        a.usuarioPorteria    || '-',
      ]),
      headStyles:         { fillColor: VERDE, textColor: [255,255,255], fontStyle: 'bold', fontSize: 8 },
      bodyStyles:         { fontSize: 8 },
      alternateRowStyles: { fillColor: [245,245,245] },
      columnStyles: {
        0:{cellWidth:10,halign:'center'},1:{cellWidth:20},2:{cellWidth:25},
        3:{cellWidth:16},4:{cellWidth:20},5:{cellWidth:38},6:{cellWidth:22},
        7:{cellWidth:18,halign:'center'},8:{cellWidth:20},9:{cellWidth:20},10:{cellWidth:20}
      }
    });
    const n = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= n; i++) {
      doc.setPage(i);
      doc.setDrawColor(...VERDE);
      doc.setLineWidth(0.3);
      doc.line(14, 200, 283, 200);
      doc.setFontSize(7);
      doc.setTextColor(120, 120, 120);
      doc.text('Sistema de Gestión de Flota', 148, 204, { align: 'center' });
      doc.text(`Página ${i} de ${n}`, 283, 204, { align: 'right' });
    }
    doc.save(`autorizaciones_${new Date().toISOString().slice(0,10)}.pdf`);
  }
}