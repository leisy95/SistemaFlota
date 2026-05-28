import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TrazabilidadService } from '../../services/trazabilidad.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-trazabilidad',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trazabilidad.html',
  styleUrls: ['./trazabilidad.scss']
})

export class TrazabilidadComponent implements OnInit {

  registros:     any[] = [];
  autorizaciones: any[] = [];
  notas:         any[] = [];

  mostrarModal    = false;
  mostrarDetalle  = false;
  mostrarNota     = false;
  editando        = false;
  editandoId:     number | null = null;
  editandoNota    = false;
  editandoNotaId: number | null = null;
  registroSeleccionado: any = null;

  filtroEstado    = '';
  filtroEntregada = '';

  form = {
    autorizacionId:   null as number | null,
    facturaRemision:  '',
    cliente:          '',
    conductor:        '',
    transportadora:   '',
    guia:             '',
    vehiculo:         '',
    pesoKilos:        null as number | null,
    valorFlete:       null as number | null,
    ajusteRecibido:   false,
    facturaEntregada: false,
    novedad:          '',
    estado:           'Pendiente'
  };

  formNota = {
    numeroNota:       '',
    cliente:          '',
    conductor:        '',
    facturaEntregada: false,
    observacion:      ''
  };

  readonly estados = [
    { value: 'Pendiente',   label: '⏳ Pendiente' },
    { value: 'EnTransito',  label: '🚚 En tránsito' },
    { value: 'Entregado',   label: '✅ Entregado' },
    { value: 'Novedad',     label: '⚠️ Novedad' },
  ];

  constructor(private trazabilidadService: TrazabilidadService) {}

  ngOnInit(): void {
    this.cargarRegistros();
    this.cargarAutorizaciones();
  }

  cargarRegistros() {
    this.trazabilidadService.obtenerTodos().subscribe({
      next: (data) => this.registros = data,
      error: (err) => console.error(err)
    });
  }

  cargarAutorizaciones() {
    this.trazabilidadService.obtenerAutorizacionesDisponibles().subscribe({
      next: (data) => this.autorizaciones = data,
      error: (err) => console.error(err)
    });
  }

  get registrosFiltrados(): any[] {
    return this.registros.filter(r => {
      const okEstado    = !this.filtroEstado    || r.estado === this.filtroEstado;
      const okEntregada = !this.filtroEntregada ||
        (this.filtroEntregada === 'si'  &&  r.facturaEntregada) ||
        (this.filtroEntregada === 'no'  && !r.facturaEntregada);
      return okEstado && okEntregada;
    });
  }

  get totalFlete(): number {
    return this.registrosFiltrados.reduce((s, r) => s + (r.valorFlete || 0), 0);
  }

  get entregados(): number {
    return this.registros.filter(r => r.facturaEntregada).length;
  }

  get pendientes(): number {
    return this.registros.filter(r => !r.facturaEntregada).length;
  }

  // =========================
  // IMPORTAR DESDE AUTORIZACIÓN
  // =========================

  importarAutorizacion(autorizacionId: number) {
    const aut = this.autorizaciones.find(a => a.id == autorizacionId);
    if (!aut) return;

    this.form.autorizacionId  = aut.id;
    this.form.conductor       = aut.conductor ?? '';
    this.form.vehiculo        = aut.vehiculo  ?? '';
    this.form.guia            = aut.numeroGuia ?? '';

    // SI TIENE FACTURAS CLIENTES TOMAR LA PRIMERA
    if (aut.facturasClientes) {
      try {
        const facturas = JSON.parse(aut.facturasClientes);
        if (facturas.length > 0) {
          this.form.facturaRemision = facturas[0].facturaRemision ?? '';
          this.form.cliente         = facturas[0].cliente         ?? '';
        }
      } catch { }
    }
  }

  // =========================
  // NUEVO / EDITAR
  // =========================

  nuevo() {
    this.editando   = false;
    this.editandoId = null;
    this.form = {
      autorizacionId: null, facturaRemision: '', cliente: '',
      conductor: '', transportadora: '', guia: '', vehiculo: '',
      pesoKilos: null, valorFlete: null,
      ajusteRecibido: false, facturaEntregada: false,
      novedad: '', estado: 'Pendiente'
    };
    this.mostrarModal = true;
  }

  editar(r: any) {
    this.editando   = true;
    this.editandoId = r.id;
    this.form = {
      autorizacionId:   r.autorizacionId   ?? null,
      facturaRemision:  r.facturaRemision,
      cliente:          r.cliente,
      conductor:        r.conductor,
      transportadora:   r.transportadora   ?? '',
      guia:             r.guia             ?? '',
      vehiculo:         r.vehiculo         ?? '',
      pesoKilos:        r.pesoKilos        ?? null,
      valorFlete:       r.valorFlete       ?? null,
      ajusteRecibido:   r.ajusteRecibido,
      facturaEntregada: r.facturaEntregada,
      novedad:          r.novedad          ?? '',
      estado:           r.estado
    };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.facturaRemision.trim()) { alert('Ingrese factura o remisión'); return; }
    if (!this.form.cliente.trim())         { alert('Ingrese el cliente');         return; }
    if (!this.form.conductor.trim())       { alert('Ingrese el conductor');       return; }

    if (this.editando && this.editandoId) {
      this.trazabilidadService.editar(this.editandoId, this.form).subscribe({
        next: () => { this.cargarRegistros(); this.cerrarModal(); },
        error: (err) => console.error(err)
      });
    } else {
      this.trazabilidadService.crear(this.form).subscribe({
        next: () => { this.cargarRegistros(); this.cerrarModal(); },
        error: (err) => console.error(err)
      });
    }
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este registro?')) return;
    this.trazabilidadService.eliminar(id).subscribe({
      next: () => this.cargarRegistros(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // DETALLE Y NOTAS
  // =========================

  verDetalle(r: any) {
    this.registroSeleccionado = r;
    this.mostrarDetalle       = true;
    this.cargarNotas(r.id);
  }

  cargarNotas(trazabilidadId: number) {
    this.trazabilidadService.obtenerNotas(trazabilidadId).subscribe({
      next: (data) => this.notas = data,
      error: (err) => console.error(err)
    });
  }

  nuevaNota() {
    this.editandoNota   = false;
    this.editandoNotaId = null;
    this.formNota = {
      numeroNota: '', cliente: '', conductor: '',
      facturaEntregada: false, observacion: ''
    };
    this.mostrarNota = true;
  }

  editarNota(n: any) {
    this.editandoNota   = true;
    this.editandoNotaId = n.id;
    this.formNota = {
      numeroNota:       n.numeroNota,
      cliente:          n.cliente      ?? '',
      conductor:        n.conductor,
      facturaEntregada: n.facturaEntregada,
      observacion:      n.observacion  ?? ''
    };
    this.mostrarNota = true;
  }

  guardarNota() {
    if (!this.formNota.numeroNota.trim()) { alert('Ingrese número de nota'); return; }
    if (!this.formNota.conductor.trim())  { alert('Ingrese el conductor');   return; }

    if (this.editandoNota && this.editandoNotaId) {
      this.trazabilidadService.editarNota(this.editandoNotaId, this.formNota).subscribe({
        next: () => {
          this.cargarNotas(this.registroSeleccionado.id);
          this.mostrarNota = false;
        },
        error: (err) => console.error(err)
      });
    } else {
      this.trazabilidadService.agregarNota(
        this.registroSeleccionado.id, this.formNota
      ).subscribe({
        next: () => {
          this.cargarNotas(this.registroSeleccionado.id);
          this.mostrarNota = false;
        },
        error: (err) => console.error(err)
      });
    }
  }

  eliminarNota(id: number) {
    if (!confirm('¿Eliminar esta nota?')) return;
    this.trazabilidadService.eliminarNota(id).subscribe({
      next: () => this.cargarNotas(this.registroSeleccionado.id),
      error: (err) => console.error(err)
    });
  }

  cerrarModal()   { this.mostrarModal   = false; }
  cerrarDetalle() { this.mostrarDetalle = false; this.registroSeleccionado = null; this.notas = []; }
  cerrarNota()    { this.mostrarNota    = false; }

  // =========================
  // BADGES
  // =========================

  getBadgeEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente':  return 'badge-pendiente';
      case 'EnTransito': return 'badge-transito';
      case 'Entregado':  return 'badge-entregado';
      case 'Novedad':    return 'badge-novedad';
      default:           return 'badge-pendiente';
    }
  }

  getLabelEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente':  return '⏳ Pendiente';
      case 'EnTransito': return '🚚 En tránsito';
      case 'Entregado':  return '✅ Entregado';
      case 'Novedad':    return '⚠️ Novedad';
      default:           return estado;
    }
  }

  // =========================
  // EXPORTAR EXCEL
  // =========================

  exportarExcel() {
    const datos = this.registrosFiltrados.map(r => ({
      'ID':                r.id,
      'Fecha':             new Date(r.fechaRegistro).toLocaleString(),
      'Factura/Remisión':  r.facturaRemision,
      'Cliente':           r.cliente,
      'Conductor':         r.conductor,
      'Transportadora':    r.transportadora   || '-',
      'Vehículo':          r.vehiculo         || '-',
      'Guía':              r.guia             || '-',
      'Peso (kg)':         r.pesoKilos        ?? '-',
      'Valor flete':       r.valorFlete       ?? '-',
      'Ajuste recibido':   r.ajusteRecibido   ? 'Sí' : 'No',
      'Factura entregada': r.facturaEntregada ? 'Sí' : 'No',
      'Fecha entrega':     r.fechaEntrega
        ? new Date(r.fechaEntrega).toLocaleString() : '-',
      'Estado':            r.estado,
      'Novedad':           r.novedad          || '-',
    }));

    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6  }, { wch: 20 }, { wch: 20 }, { wch: 22 },
      { wch: 22 }, { wch: 20 }, { wch: 12 }, { wch: 15 },
      { wch: 10 }, { wch: 15 }, { wch: 15 }, { wch: 15 },
      { wch: 20 }, { wch: 12 }, { wch: 30 }
    ];

    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Trazabilidad');
    XLSX.writeFile(libro, `trazabilidad_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}