import {
  Component, OnInit, AfterViewInit, OnDestroy,
  ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil, timeout, catchError } from 'rxjs/operators';
import { TimeoutError, throwError } from 'rxjs';

import { ConductoresService }    from '../../../core/services/conductores.service';
import { VehiculosService }      from '../../../core/services/vehiculos.service';
import { AutorizacionesService } from '../../../core/services/autorizaciones.service';
import { ConfiguracionService }  from '../../../core/services/configuracion.service';
import { PdfService }            from '../../../core/services/pdf.service';
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

  // ── Paginación ────────────────────────────────────────────────────────────
  paginaActual = 1;
  porPagina    = 10;
  totalPaginas = 1;

  get autorizacionesPaginadas(): any[] {
    const inicio = (this.paginaActual - 1) * this.porPagina;
    return (this.autorizacionesFiltradas ?? []).slice(inicio, inicio + this.porPagina);
  }

  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) { this.paginaActual++; this.cdr.markForCheck(); }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) { this.paginaActual--; this.cdr.markForCheck(); }
  }

  form = {
    vehiculoId: 0, destinoCompleto: '', cantidadClientes: 0,
    pesoKilos: 0, tipoVuelta: '', descripcionCarga: '', numeroGuia: ''
  };

  facturasClientes: { facturaRemision: string; cliente: string; pesoKilos: any }[] = [];
  facturasEditar:   { facturaRemision: string; cliente: string; pesoKilos: any }[] = [];

  pesoBaseClientes = 0;

  guiaGenerada     = '';
  usuarioFirma     = '';
  observacionFirma = '';
  notificacion:    string | null = null;
  autorizaciones:  any[] = [];
  autorizacionesFiltradas: any[] = [];

  vistaLista    = true;
  rolUsuario    = '';
  nombreEmpresa = 'la empresa';

  mostrarModalLlegada           = false;
  mostrarModalConfirmar         = false;
  mostrarModalEditar            = false;
  mostrarModalDetalle           = false;
  mostrarModalSalidaRapida      = false;
  mostrarModalLlegadaRapida     = false;
  mostrarModalSelectorLlegada   = false;
  mostrarModalSelectorSalida    = false;
  autorizacionLlegada:   any = null;
  autorizacionEditando:  any = null;
  autorizacionDetalle:   any = null;

  formSalidaRapida = {
    conductorId: 0, vehiculoId: 0, tipoVuelta: 'Mensajería', destinoCompleto: ''
  };

  formLlegadaRapida = {
    conductorId: 0, vehiculoId: 0, tipoVuelta: 'Mensajería', destinoCompleto: '',
    kilometrajeFinal: null as number | null, novedadesViaje: '', estadoVehiculo: 'Bueno'
  };

  guardandoRapido = false;

  formLlegada = {
    kilometrajeFinal: null as number | null, novedadesViaje: '', estadoVehiculo: 'Bueno'
  };

  formEditar = {
    conductorId:      0,
    vehiculoId:       0,
    destinoCompleto:  '',
    tipoVuelta:       '',
    descripcionCarga: '',
    cantidadClientes: 0,
    pesoKilos:        0,
    numeroGuia:       ''
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
      next: (data: any) => {
        if (data.nombreEmpresa?.trim()) this.nombreEmpresa = data.nombreEmpresa;
        this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  ngAfterViewInit(): void {}
  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }

  get puedeAccesoRapido(): boolean {
    return ['Admin', 'Jefe', 'Conductor', 'Vendedor'].includes(this.rolUsuario);
  }

  // ── Autorizaciones activas sin llegada ────────────────────────────────────
  get autorizacionesActivasSinLlegada(): any[] {
    return (this.autorizaciones ?? []).filter(a =>
      a.estado === 'Autorizado' && !a.estadoLlegada
    );
  }

  // ── Autorizaciones activas sin salida ─────────────────────────────────────
  get autorizacionesActivasSinSalida(): any[] {
    return (this.autorizaciones ?? []).filter(a =>
      a.estado === 'Autorizado' && !a.fechaSalidaReal
    );
  }

  abrirSelectorLlegada() {
    const activas = this.autorizacionesActivasSinLlegada;
    if (activas.length === 0) {
      this.abrirLlegadaRapida();
    } else if (activas.length === 1) {
      this.abrirReporteLlegada(activas[0]);
    } else {
      this.mostrarModalSelectorLlegada = true;
      this.cdr.markForCheck();
    }
  }

  abrirSelectorSalida() {
    const activas = this.autorizacionesActivasSinSalida;
    if (activas.length === 0) {
      this.abrirSalidaRapida();
    } else if (activas.length === 1) {
      this.confirmarSalida(activas[0]);
    } else {
      this.mostrarModalSelectorSalida = true;
      this.cdr.markForCheck();
    }
  }

  seleccionarSalidaConductor(autorizacion: any) {
    this.mostrarModalSelectorSalida = false;
    this.confirmarSalida(autorizacion);
    this.cdr.markForCheck();
  }

  seleccionarLlegadaConductor(autorizacion: any) {
    this.mostrarModalSelectorLlegada = false;
    this.abrirReporteLlegada(autorizacion);
    this.cdr.markForCheck();
  }

  getIniciales(nombre: string): string {
    return (nombre ?? '??').split(' ').slice(0, 2).map((n: string) => n[0]).join('').toUpperCase();
  }

  getBadgeSalida(a: any): string {
    if (a.fechaSalidaReal) return 'ruta';
    if (a.fechaPorteria)   return 'sinconfirmar';
    return 'sinregistrar';
  }

  abrirSalidaRapida() {
    this.formSalidaRapida = { conductorId: 0, vehiculoId: 0, tipoVuelta: 'Mensajería', destinoCompleto: '' };
    this.guardandoRapido = false;
    this.mostrarModalSalidaRapida = true;
    this.cdr.markForCheck();
  }

  abrirLlegadaRapida() {
    this.formLlegadaRapida = {
      conductorId: 0, vehiculoId: 0, tipoVuelta: 'Mensajería', destinoCompleto: '',
      kilometrajeFinal: null, novedadesViaje: '', estadoVehiculo: 'Bueno'
    };
    this.guardandoRapido = false;
    this.mostrarModalLlegadaRapida = true;
    this.cdr.markForCheck();
  }

  guardarSalidaRapida() {
    if (!this.formSalidaRapida.conductorId) { alert('Seleccione un conductor'); return; }
    if (!this.formSalidaRapida.vehiculoId)  { alert('Seleccione un vehículo'); return; }
    this.guardandoRapido = true;
    this.autorizacionesService.salidaRapida({
      conductorId: this.formSalidaRapida.conductorId,
      vehiculoId:  this.formSalidaRapida.vehiculoId,
      tipoVuelta:  this.formSalidaRapida.tipoVuelta,
      destinoCompleto: this.formSalidaRapida.destinoCompleto || undefined
    }).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => {
        alert(`Error: ${JSON.stringify(err.error ?? err.message)}`);
        this.guardandoRapido = false; this.cdr.markForCheck();
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.guardandoRapido = false;
        this.mostrarModalSalidaRapida = false;
        this.mostrarNotificacion('🚛 Salida registrada — WhatsApp enviado');
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  guardarLlegadaRapida() {
    if (!this.formLlegadaRapida.conductorId) { alert('Seleccione un conductor'); return; }
    if (!this.formLlegadaRapida.vehiculoId)  { alert('Seleccione un vehículo'); return; }
    this.guardandoRapido = true;
    this.autorizacionesService.llegadaRapida({
      conductorId: this.formLlegadaRapida.conductorId,
      vehiculoId:  this.formLlegadaRapida.vehiculoId,
      tipoVuelta:  this.formLlegadaRapida.tipoVuelta,
      destinoCompleto:  this.formLlegadaRapida.destinoCompleto || undefined,
      kilometrajeFinal: this.formLlegadaRapida.kilometrajeFinal,
      novedadesViaje:   this.formLlegadaRapida.novedadesViaje,
      estadoVehiculo:   this.formLlegadaRapida.estadoVehiculo
    }).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => {
        alert(`Error: ${JSON.stringify(err.error ?? err.message)}`);
        this.guardandoRapido = false; this.cdr.markForCheck();
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.guardandoRapido = false;
        this.mostrarModalLlegadaRapida = false;
        this.mostrarNotificacion('🏁 Llegada registrada — WhatsApp enviado');
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  aplicarFiltros(): void {
    if (!Array.isArray(this.autorizaciones)) return;
    const q = this.filtroBusqueda.toLowerCase();
    let base: any[];
    switch (this.rolUsuario) {
      case 'Facturacion': base = [...this.autorizaciones]; break;
      case 'Bodega':      base = this.autorizaciones.filter(a => a.estado === 'Bodega'); break;
      case 'Porteria':    base = this.autorizaciones.filter(a =>
        a.estado === 'Porteria' || (a.estado === 'Autorizado' && a.estadoLlegada === 'ReportadaLlegada')); break;
      case 'Vendedor':    base = this.autorizaciones.filter(a => a.estado === 'Autorizado'); break;
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
    // ── Paginación ──────────────────────────────────────────────────────────
    this.paginaActual = 1;
    this.totalPaginas = Math.ceil(this.autorizacionesFiltradas.length / this.porPagina);
    this.cdr.markForCheck();
  }

  limpiarFiltros(): void {
    this.filtroBusqueda = ''; this.filtroEstado = '';
    this.filtroTipo = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  private actualizarFiltradas(): void { this.aplicarFiltros(); }

  get puedeCrear():  boolean { return !['Bodega', 'Porteria'].includes(this.rolUsuario); }
  get puedeEditar(): boolean { return ['Admin', 'Jefe', 'Facturacion', 'Bodega'].includes(this.rolUsuario); }

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

  get pesoTotalFacturas(): number {
    return this.facturasClientes.reduce((sum, f) => {
      const val = parseFloat(String(f.pesoKilos ?? '').replace(',', '.'));
      return sum + (isNaN(val) ? 0 : val);
    }, 0);
  }

  recalcularPeso(): void {
    const total = this.pesoTotalFacturas;
    if (total > 0) this.form.pesoKilos = total;
    this.cdr.markForCheck();
  }

  get pesoTotalFacturasEditar(): number {
    return this.facturasEditar.reduce((sum, f) => {
      const val = parseFloat(String(f.pesoKilos ?? '').replace(',', '.'));
      return sum + (isNaN(val) ? 0 : val);
    }, 0);
  }

  recalcularPesoEditar(): void {
    this.formEditar.pesoKilos = parseFloat(
      (this.pesoBaseClientes + this.pesoTotalFacturasEditar).toFixed(2)
    );
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
      next: (res: any) => {
        this.autorizaciones = Array.isArray(res) ? res : (res.data ?? []);
        this.actualizarFiltradas();
      },
      error: (err) => console.error(err)
    });
  }

  verDetalle(autorizacion: any) {
    this.autorizacionDetalle = autorizacion;
    this.mostrarModalDetalle = true;
    this.cdr.markForCheck();
  }

  abrirEditar(autorizacion: any) {
    this.autorizacionEditando = autorizacion;
    this.formEditar = {
      conductorId:      autorizacion.conductorId      ?? 0,
      vehiculoId:       autorizacion.vehiculoId       ?? 0,
      destinoCompleto:  autorizacion.destinoCompleto  ?? '',
      tipoVuelta:       autorizacion.tipoVuelta       ?? '',
      descripcionCarga: autorizacion.descripcionCarga ?? '',
      cantidadClientes: autorizacion.cantidadClientes ?? 0,
      pesoKilos:        autorizacion.pesoKilos        ?? 0,
      numeroGuia:       autorizacion.numeroGuia       ?? ''
    };
    this.facturasEditar = this.obtenerFacturas(autorizacion.facturasClientes);
    const pesoFacturasYaGuardadas = this.facturasEditar.reduce((sum, f) => {
      const val = parseFloat(String(f.pesoKilos ?? '').replace(',', '.'));
      return sum + (isNaN(val) ? 0 : val);
    }, 0);
    this.pesoBaseClientes = parseFloat(
      Math.max(0, (autorizacion.pesoKilos ?? 0) - pesoFacturasYaGuardadas).toFixed(2)
    );
    this.mostrarModalEditar = true;
    this.cdr.markForCheck();
  }

  guardarEdicion() {
    if (!this.autorizacionEditando) return;
    if (!this.formEditar.conductorId) { alert('Seleccione un conductor'); return; }
    if (!this.formEditar.vehiculoId)  { alert('Seleccione un vehículo'); return; }
    const pesoFinal = parseFloat(
      (this.pesoBaseClientes + this.pesoTotalFacturasEditar || this.formEditar.pesoKilos).toFixed(2)
    );
    const datos = {
      conductorId:      this.formEditar.conductorId,
      vehiculoId:       this.formEditar.vehiculoId,
      destinoCompleto:  this.formEditar.destinoCompleto,
      cantidadClientes: this.formEditar.cantidadClientes,
      pesoKilos:        pesoFinal,
      tipoVuelta:       this.formEditar.tipoVuelta,
      descripcionCarga: this.formEditar.descripcionCarga,
      numeroGuia:       this.formEditar.numeroGuia,
      facturasClientes: this.facturasEditar.length > 0
        ? JSON.stringify(this.facturasEditar) : null
    };
    this.autorizacionesService.editar(this.autorizacionEditando.id, datos)
      .pipe(timeout(15000), takeUntil(this.destroy$),
        catchError(err => {
          alert(`Error: ${JSON.stringify(err.error ?? err.message)}`);
          return throwError(() => err);
        })
      ).subscribe({
        next: () => {
          this.mostrarModalEditar = false;
          this.mostrarNotificacion('✅ Autorización actualizada');
          this.obtenerAutorizaciones();
          this.cdr.markForCheck();
        },
        error: () => {}
      });
  }

  agregarFacturaEditar() {
    this.facturasEditar = [...this.facturasEditar, { facturaRemision: '', cliente: '', pesoKilos: null }];
    this.cdr.markForCheck();
  }

  eliminarFacturaEditar(i: number) {
    this.facturasEditar = this.facturasEditar.filter((_, idx) => idx !== i);
    this.recalcularPesoEditar();
    this.cdr.markForCheck();
  }

  abrirReporteLlegada(autorizacion: any) {
    this.autorizacionLlegada = autorizacion;
    this.formLlegada = { kilometrajeFinal: null, novedadesViaje: '', estadoVehiculo: 'Bueno' };
    this.mostrarModalLlegada = true;
    this.cdr.markForCheck();
  }

  guardarReporteLlegada() {
    this.autorizacionesService.reportarLlegada(this.autorizacionLlegada.id, {
      kilometrajeFinal: this.formLlegada.kilometrajeFinal,
      novedadesViaje:   this.formLlegada.novedadesViaje,
      estadoVehiculo:   this.formLlegada.estadoVehiculo
    }).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => {
        alert(`Error: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.mostrarModalLlegada = false;
        this.mostrarNotificacion('✅ Llegada reportada — WhatsApp enviado');
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
      },
      error: () => {}
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
      catchError(err => {
        alert(`Error: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
    ).subscribe({
      next: () => {
        this.mostrarModalConfirmar = false;
        this.mostrarNotificacion('✅ Llegada confirmada por portería');
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  seleccionarConductor(conductor: any) {
    this.conductorSeleccionado = conductor;
    this.pasoActual = 2;
    this.vistaLista = false;
    this.cdr.markForCheck();
  }

  generarGuia() {
    this.autorizacionesService.generarGuia().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data: any) => {
        this.form.numeroGuia = data.guia;
        this.guiaGenerada = data.guia;
        this.cdr.markForCheck();
      },
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
      facturasClientes: this.facturasClientes.length > 0
        ? JSON.stringify(this.facturasClientes) : null
    };
    this.autorizacionesService.crear(datos).pipe(timeout(15000), takeUntil(this.destroy$),
      catchError(err => {
        alert(err instanceof TimeoutError ? 'Timeout' : `Error: ${JSON.stringify(err.error ?? err.message)}`);
        return throwError(() => err);
      })
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
      next: (data) => {
        this.autorizacionActual = data;
        this.usuarioFirma = ''; this.observacionFirma = '';
        this.pasoActual = 4; this.cdr.markForCheck();
      },
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
      next: (data) => {
        this.autorizacionActual = data;
        this.usuarioFirma = ''; this.observacionFirma = '';
        this.pasoActual = 5;
        this.mostrarNotificacion('Salida autorizada por Bodega');
        this.cdr.markForCheck();
      },
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
        this.mostrarNotificacion(`✅ ${this.conductorSeleccionado.nombre} — ¡Buen viaje! WhatsApp enviado`);
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
        setTimeout(() => this.resetear(), 3000);
      },
      error: () => {}
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

  confirmarSalida(autorizacion: any) {
    if (!confirm(`¿Confirmar salida en ruta de ${autorizacion.conductor?.nombre}?`)) return;
    this.autorizacionesService.confirmarSalida(autorizacion.id).pipe(
      timeout(15000), takeUntil(this.destroy$),
      catchError(err => { alert(`Error: ${JSON.stringify(err.error ?? err.message)}`); return throwError(() => err); })
    ).subscribe({
      next: () => {
        this.mostrarNotificacion('🚛 Salida en ruta confirmada — WhatsApp enviado');
        this.obtenerAutorizaciones(); this.cdr.markForCheck();
      },
      error: () => {}
    });
  }

  resetear() {
    this.pasoActual = 1; this.conductorSeleccionado = null;
    this.autorizacionActual = null; this.vistaLista = true;
    this.facturasClientes = []; this.facturasEditar = [];
    this.guiaGenerada = ''; this.pesoBaseClientes = 0;
    this.form = {
      vehiculoId: 0, destinoCompleto: '', cantidadClientes: 0,
      pesoKilos: 0, tipoVuelta: '', descripcionCarga: '', numeroGuia: ''
    };
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

  async descargarPDF(autorizacion: any) { await this.pdfService.generarPDFAutorizacion(autorizacion); }

  exportarExcel() {
    const datos = this.autorizacionesFiltradas.map(a => ({
      'ID': a.id, 'Fecha': new Date(a.fechaCreacion).toLocaleString(),
      'Conductor': a.conductor?.nombre ?? '-', 'Vehículo': a.vehiculo?.placa ?? '-',
      'Tipo': a.tipoVuelta ?? '-', 'Destino': a.destinoCompleto || 'Mensajería',
      'Estado': a.estado, 'Estado Llegada': a.estadoLlegada ?? 'En ruta',
      'Km Final': a.kilometrajeFinal ?? '-', 'Novedades': a.novedadesViaje || '-',
      'Estado Vehículo': a.estadoVehiculoLlegada ?? '-',
      'Facturación': a.usuarioFacturacion || '-',
      'Bodega': a.usuarioBodega || '-',
      'Portería': a.usuarioPorteria || '-',
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
      bodyStyles: { fontSize: 8 },
      alternateRowStyles: { fillColor: [245,245,245] }
    });
    doc.save(`autorizaciones_${new Date().toISOString().slice(0,10)}.pdf`);
  }
}