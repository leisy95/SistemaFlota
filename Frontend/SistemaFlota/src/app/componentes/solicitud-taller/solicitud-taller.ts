import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SolicitudTallerService } from '../../services/solicitud-taller.service';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-solicitud-taller',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './solicitud-taller.html',
  styleUrls: ['./solicitud-taller.scss']
})

export class SolicitudTallerComponent implements OnInit {

  solicitudes:  any[] = [];
  conductores:  any[] = [];
  vehiculos:    any[] = [];

  mostrarModal        = false;
  mostrarAut          = false;
  mostrarFactura      = false;
  mostrarDetalle      = false;
  seleccionado:       any = null;
  accionAut           = '';
  fotoPreview:        string | null = null;
  fotoSeleccionada:   File | null = null;

  filtroEstado = '';

  form = {
    conductorId:        0,
    vehiculoId:         0,
    tipoMantenimiento:  '',
    descripcionProblema:'',
    kilometraje:        null as number | null
  };

  formAut = {
    autorizadoPor: '',
    observacion:   ''
  };

  formFactura = {
    numeroFacturaTaller: '',
    valorFactura:        null as number | null,
    fechaFactura:        '',
    facturaValidada:     false,
    observacionFactura:  ''
  };

  readonly tiposMantenimiento = [
    'Preventivo', 'Correctivo', 'Cambio de aceite',
    'Cambio de frenos', 'Cambio de llantas',
    'Revisión general', 'Mantenimiento eléctrico',
    'Mantenimiento de suspensión', 'Otro'
  ];

  constructor(
    private solicitudService:   SolicitudTallerService,
    private conductoresService: ConductoresService,
    private vehiculosService:   VehiculosService
  ) {}

  ngOnInit(): void {
    this.cargarSolicitudes();
    this.cargarConductores();
    this.cargarVehiculos();
  }

  cargarSolicitudes() {
    this.solicitudService.obtenerTodos().subscribe({
      next: (data) => this.solicitudes = data,
      error: (err) => console.error(err)
    });
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

  get solicitudesFiltradas(): any[] {
    return this.solicitudes.filter(s =>
      !this.filtroEstado || s.estado === this.filtroEstado
    );
  }

  get pendientes(): number {
    return this.solicitudes.filter(s => s.estado === 'Pendiente').length;
  }

  get enTaller(): number {
    return this.solicitudes.filter(s => s.estado === 'EnTaller').length;
  }

  get finalizados(): number {
    return this.solicitudes.filter(s => s.estado === 'Finalizado').length;
  }

  get pendientesFactura(): number {
    return this.solicitudes.filter(s =>
      s.estado === 'Finalizado' && !s.facturaValidada
    ).length;
  }

  // =========================
  // FOTO ODÓMETRO
  // =========================

  seleccionarFoto(event: any) {
    const archivo = event.target.files[0];
    if (!archivo) return;
    this.fotoSeleccionada = archivo;
    const reader = new FileReader();
    reader.onload = (e: any) => this.fotoPreview = e.target.result;
    reader.readAsDataURL(archivo);
  }

  eliminarFoto() {
    this.fotoSeleccionada = null;
    this.fotoPreview      = null;
  }

  // =========================
  // NUEVO
  // =========================

  nuevo() {
    this.form = {
      conductorId: 0, vehiculoId: 0,
      tipoMantenimiento: '', descripcionProblema: '',
      kilometraje: null
    };
    this.fotoSeleccionada = null;
    this.fotoPreview      = null;
    this.mostrarModal     = true;
  }

  guardar() {
    if (!this.form.conductorId)              { alert('Seleccione un conductor');        return; }
    if (!this.form.vehiculoId)               { alert('Seleccione un vehículo');         return; }
    if (!this.form.tipoMantenimiento)        { alert('Seleccione tipo de mantenimiento'); return; }
    if (!this.form.descripcionProblema.trim()){ alert('Describa el problema');           return; }

    const formData = new FormData();
    formData.append('ConductorId',        this.form.conductorId.toString());
    formData.append('VehiculoId',         this.form.vehiculoId.toString());
    formData.append('TipoMantenimiento',  this.form.tipoMantenimiento);
    formData.append('DescripcionProblema',this.form.descripcionProblema);

    if (this.form.kilometraje)
      formData.append('Kilometraje', this.form.kilometraje.toString());

    if (this.fotoSeleccionada)
      formData.append('FotoOdometro', this.fotoSeleccionada);

    this.solicitudService.crear(formData).subscribe({
      next: () => { this.cargarSolicitudes(); this.cerrarModal(); },
      error: (err) => console.error(err)
    });
  }

  // =========================
  // AUTORIZAR / RECHAZAR
  // =========================

  abrirAutorizar(s: any) {
    this.seleccionado = s;
    this.accionAut    = 'autorizar';
    this.formAut      = { autorizadoPor: '', observacion: '' };
    this.mostrarAut   = true;
  }

  abrirRechazar(s: any) {
    this.seleccionado = s;
    this.accionAut    = 'rechazar';
    this.formAut      = { autorizadoPor: '', observacion: '' };
    this.mostrarAut   = true;
  }

  confirmarAut() {
    if (!this.formAut.autorizadoPor.trim()) { alert('Ingrese su nombre'); return; }

    const accion$ = this.accionAut === 'autorizar'
      ? this.solicitudService.autorizar(this.seleccionado.id, this.formAut)
      : this.solicitudService.rechazar(this.seleccionado.id, this.formAut);

    accion$.subscribe({
      next: () => { this.cargarSolicitudes(); this.cerrarAut(); },
      error: (err) => console.error(err)
    });
  }

  // =========================
  // MARCAR EN TALLER
  // =========================

  marcarEnTaller(id: number) {
    if (!confirm('¿Marcar como En Taller?')) return;
    this.solicitudService.marcarEnTaller(id).subscribe({
      next: () => this.cargarSolicitudes(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // FACTURA TALLER
  // =========================

  abrirFactura(s: any) {
    this.seleccionado = s;
    this.formFactura  = {
      numeroFacturaTaller: s.numeroFacturaTaller || '',
      valorFactura:        s.valorFactura        || null,
      fechaFactura:        s.fechaFactura
        ? new Date(s.fechaFactura).toISOString().slice(0, 10) : '',
      facturaValidada:     s.facturaValidada     || false,
      observacionFactura:  s.observacionFactura  || ''
    };
    this.mostrarFactura = true;
  }

  guardarFactura() {
    if (!this.formFactura.numeroFacturaTaller?.trim()) {
      alert('Ingrese el número de factura'); return;
    }

    this.solicitudService.registrarFactura(
      this.seleccionado.id, this.formFactura
    ).subscribe({
      next: () => { this.cargarSolicitudes(); this.cerrarFactura(); },
      error: (err) => console.error(err)
    });
  }

  // =========================
  // DETALLE
  // =========================

  verDetalle(s: any) {
    this.seleccionado  = s;
    this.mostrarDetalle = true;
  }

  // =========================
  // ELIMINAR
  // =========================

  eliminar(id: number) {
    if (!confirm('¿Eliminar esta solicitud?')) return;
    this.solicitudService.eliminar(id).subscribe({
      next: () => this.cargarSolicitudes(),
      error: (err) => console.error(err)
    });
  }

  cerrarModal()   { this.mostrarModal   = false; }
  cerrarAut()     { this.mostrarAut     = false; this.seleccionado = null; }
  cerrarFactura() { this.mostrarFactura = false; this.seleccionado = null; }
  cerrarDetalle() { this.mostrarDetalle = false; this.seleccionado = null; }

  // =========================
  // BADGES
  // =========================

  getBadgeEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente':  return 'badge-pendiente';
      case 'Autorizado': return 'badge-autorizado';
      case 'EnTaller':   return 'badge-taller';
      case 'Finalizado': return 'badge-finalizado';
      case 'Rechazado':  return 'badge-rechazado';
      default:           return 'badge-pendiente';
    }
  }

  getLabelEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente':  return '⏳ Pendiente';
      case 'Autorizado': return '✅ Autorizado';
      case 'EnTaller':   return '🔧 En Taller';
      case 'Finalizado': return '✔️ Finalizado';
      case 'Rechazado':  return '❌ Rechazado';
      default:           return estado;
    }
  }

  // =========================
  // EXPORTAR EXCEL
  // =========================

  exportarExcel() {
    const datos = this.solicitudesFiltradas.map(s => ({
      'ID':                s.id,
      'Fecha solicitud':   new Date(s.fechaSolicitud).toLocaleString(),
      'Conductor':         s.conductor?.nombre     ?? '-',
      'Vehículo':          s.vehiculo?.placa       ?? '-',
      'Tipo':              s.tipoMantenimiento,
      'Descripción':       s.descripcionProblema,
      'Kilometraje':       s.kilometraje           ?? '-',
      'Estado':            s.estado,
      'Autorizado por':    s.autorizadoPor         || '-',
      'Obs. autorización': s.observacionAut        || '-',
      'Fecha autorización':s.fechaAutorizacion
        ? new Date(s.fechaAutorizacion).toLocaleString() : '-',
      'Nº Factura taller': s.numeroFacturaTaller   || '-',
      'Valor factura':     s.valorFactura          ?? '-',
      'Fecha factura':     s.fechaFactura
        ? new Date(s.fechaFactura).toLocaleDateString() : '-',
      'Factura validada':  s.facturaValidada ? 'Sí' : 'No',
      'Obs. factura':      s.observacionFactura    || '-',
    }));

    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6  }, { wch: 20 }, { wch: 22 }, { wch: 12 },
      { wch: 22 }, { wch: 35 }, { wch: 12 }, { wch: 12 },
      { wch: 20 }, { wch: 25 }, { wch: 20 }, { wch: 18 },
      { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 25 }
    ];

    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Solicitudes Taller');
    XLSX.writeFile(libro, `solicitudes_taller_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}