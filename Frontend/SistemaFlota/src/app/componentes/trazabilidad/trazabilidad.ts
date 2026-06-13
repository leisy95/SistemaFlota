import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TrazabilidadService } from '../../services/trazabilidad.service';
import { PermisosService } from '../../services/permisos.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-trazabilidad',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './trazabilidad.html',
  styleUrls: ['./trazabilidad.scss']
})
export class TrazabilidadComponent implements OnInit {

  registros: any[] = [];
  registrosFiltrados: any[] = [];
  autorizaciones: any[] = [];
  notas: any[] = [];
  facturasDisponibles: any[] = [];

  mostrarModal = false;
  mostrarDetalle = false;
  mostrarNota = false;
  editando = false;
  editandoId: number | null = null;
  editandoNota = false;
  editandoNotaId: number | null = null;
  registroSeleccionado: any = null;

  filtroEstado = '';
  filtroEntregada = '';
  filtroBusqueda = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';
  filtroTipo = 'FE';
  mostrarSelectorTipo = false;

  readonly tiposFiltro = [
    { value: 'FE', label: 'FE — Facturas' },
    { value: 'COT', label: 'COT — Cotizaciones' },
    { value: 'RE', label: 'RE — Remisiones' },
    { value: 'NC', label: 'NC — Notas crédito' },
    { value: '', label: 'Todas' },
  ];

  form = {
    autorizacionId: null as number | null,
    facturaRemision: '',
    cliente: '',
    conductor: '',
    transportadora: '',
    guia: '',
    vehiculo: '',
    pesoKilos: null as number | null,
    valorFlete: null as number | null,
    acuseRecibido: false,
    facturaEntregada: false,
    novedad: '',
    estado: 'Pendiente'
  };

  formNota = {
    numeroNota: '', cliente: '', conductor: '',
    facturaEntregada: false, observacion: ''
  };

  readonly estados = [
    { value: 'Pendiente', label: '⏳ Pendiente' },
    { value: 'EnTransito', label: '🚚 En tránsito' },
    { value: 'Entregado', label: '✅ Entregado' },
    { value: 'Novedad', label: '⚠️ Novedad' },
  ];

  get puedeCrear(): boolean { return this.permisosService.puedeCrear('trazabilidad'); }
  get puedeEditar(): boolean { return this.permisosService.puedeEditar('trazabilidad'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('trazabilidad'); }

  get totalFlete(): number { return this.registrosFiltrados.reduce((s, r) => s + (r.valorFlete || 0), 0); }
  get entregados(): number { return this.registros.filter(r => r.facturaEntregada).length; }
  get pendientes(): number { return this.registros.filter(r => !r.facturaEntregada).length; }

  constructor(
    private trazabilidadService: TrazabilidadService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    this.cargarRegistros();
    this.cargarAutorizaciones();
  }

  cargarRegistros() {
    this.trazabilidadService.obtenerTodos().subscribe({
      next: (data) => {
        this.registros = data.sort((a: any, b: any) => {
          const extraerNumero = (s: string) => {
            const match = (s ?? '').toString().match(/\d+/);
            return match ? parseInt(match[0], 10) : 0;
          };
          const fa = (a.facturaRemision ?? '').toString().toUpperCase();
          const fb = (b.facturaRemision ?? '').toString().toUpperCase();
          const prefA = fa.replace(/[\d\s\-]/g, '').trim();
          const prefB = fb.replace(/[\d\s\-]/g, '').trim();
          if (prefA !== prefB) return prefA.localeCompare(prefB);
          return extraerNumero(fa) - extraerNumero(fb);
        });
        this.aplicarFiltros();
      },
      error: (err) => console.error(err)
    });
  }

  cargarAutorizaciones() {
    this.trazabilidadService.obtenerAutorizacionesDisponibles().subscribe({
      next: (data) => this.autorizaciones = data,
      error: (err) => console.error(err)
    });
  }

  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.registrosFiltrados = this.registros.filter(r => {
      const okEstado = !this.filtroEstado || r.estado === this.filtroEstado;
      const okEntregada = !this.filtroEntregada ||
        (this.filtroEntregada === 'si' && r.facturaEntregada) ||
        (this.filtroEntregada === 'no' && !r.facturaEntregada);
      const fecha = new Date(r.fechaRegistro);
      const okDesde = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq = !q ||
        r.facturaRemision?.toLowerCase().includes(q) ||
        r.cliente?.toLowerCase().includes(q) ||
        r.conductor?.toLowerCase().includes(q) ||
        r.vehiculo?.toLowerCase().includes(q) ||
        r.guia?.toLowerCase().includes(q);
      const okTipo = !this.filtroTipo ||
        (r.facturaRemision ?? '').toString().toUpperCase().startsWith(this.filtroTipo);
      return okEstado && okEntregada && okDesde && okHasta && okBusq && okTipo;
    });
  }

  limpiarFiltros() {
    this.filtroEstado = '';
    this.filtroEntregada = '';
    this.filtroBusqueda = '';
    this.filtroFechaDesde = '';
    this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  importarAutorizacion(autorizacionId: number) {
    const aut = this.autorizaciones.find(a => a.id == autorizacionId);
    if (!aut) { this.facturasDisponibles = []; return; }

    this.form.autorizacionId = aut.id;
    this.form.conductor = aut.conductor ?? '';
    this.form.vehiculo = aut.vehiculo ?? '';
    this.form.guia = aut.numeroGuia ?? '';
    this.form.facturaRemision = '';
    this.form.cliente = '';
    this.form.pesoKilos = null;

    if (aut.facturasClientes) {
      try {
        this.facturasDisponibles = JSON.parse(aut.facturasClientes);
      } catch { this.facturasDisponibles = []; }
    } else {
      this.facturasDisponibles = [];
    }
  }

  seleccionarFactura(index: number) {
    if (index < 0 || index >= this.facturasDisponibles.length) return;
    const f = this.facturasDisponibles[index];
    this.form.facturaRemision = f.facturaRemision ?? '';
    this.form.cliente = f.cliente ?? '';
    this.form.pesoKilos = f.pesoKilos ?? null;
  }

  nuevo() {
    this.editando = false; this.editandoId = null;
    this.facturasDisponibles = [];
    this.form = {
      autorizacionId: null,
      facturaRemision: '',
      cliente: '',
      conductor: '',
      transportadora: '',
      guia: '',
      vehiculo: '',
      pesoKilos: null,
      valorFlete: null,
      acuseRecibido: false,
      facturaEntregada: false,
      novedad: '',
      estado: 'Pendiente'
    };
    this.mostrarModal = true;
  }

  editar(r: any) {
    this.editando = true; this.editandoId = r.id;
    this.facturasDisponibles = [];
    this.form = {
      autorizacionId: r.autorizacionId ?? null,
      facturaRemision: r.facturaRemision,
      cliente: r.cliente,
      conductor: r.conductor,
      transportadora: r.transportadora ?? '',
      guia: r.guia ?? '',
      vehiculo: r.vehiculo ?? '',
      pesoKilos: r.pesoKilos ?? null,
      valorFlete: r.valorFlete ?? null,
      acuseRecibido: r.ajusteRecibido,
      facturaEntregada: r.facturaEntregada,
      novedad: r.novedad ?? '',
      estado: r.estado
    };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.facturaRemision.trim()) { alert('Ingrese factura o remisión'); return; }
    if (!this.form.cliente.trim()) { alert('Ingrese el cliente'); return; }
    if (!this.form.conductor.trim()) { alert('Ingrese el conductor'); return; }

    // ✅ Mapear acuseRecibido → ajusteRecibido para el backend
    const dto = {
      autorizacionId: this.form.autorizacionId,
      facturaRemision: this.form.facturaRemision,
      cliente: this.form.cliente,
      conductor: this.form.conductor,
      transportadora: this.form.transportadora,
      guia: this.form.guia,
      vehiculo: this.form.vehiculo,
      pesoKilos: this.form.pesoKilos,
      valorFlete: this.form.valorFlete,
      ajusteRecibido: this.form.acuseRecibido,
      facturaEntregada: this.form.facturaEntregada,
      novedad: this.form.novedad,
      estado: this.form.estado
    };

    const peticion = this.editando && this.editandoId
      ? this.trazabilidadService.editar(this.editandoId, dto)
      : this.trazabilidadService.crear(dto);

    peticion.subscribe({
      next: () => { this.cargarRegistros(); this.cerrarModal(); },
      error: (err) => console.error(err)
    });
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este registro?')) return;
    this.trazabilidadService.eliminar(id).subscribe({
      next: () => this.cargarRegistros(),
      error: (err) => console.error(err)
    });
  }

  verDetalle(r: any) {
    this.registroSeleccionado = r;
    this.mostrarDetalle = true;
    this.cargarNotas(r.id);
  }

  cargarNotas(trazabilidadId: number) {
    this.trazabilidadService.obtenerNotas(trazabilidadId).subscribe({
      next: (data) => this.notas = data,
      error: (err) => console.error(err)
    });
  }

  nuevaNota() {
    this.editandoNota = false; this.editandoNotaId = null;
    this.formNota = { numeroNota: '', cliente: '', conductor: '', facturaEntregada: false, observacion: '' };
    this.mostrarNota = true;
  }

  editarNota(n: any) {
    this.editandoNota = true; this.editandoNotaId = n.id;
    this.formNota = {
      numeroNota: n.numeroNota,
      cliente: n.cliente ?? '',
      conductor: n.conductor,
      facturaEntregada: n.facturaEntregada,
      observacion: n.observacion ?? ''
    };
    this.mostrarNota = true;
  }

  guardarNota() {
    if (!this.formNota.numeroNota.trim()) { alert('Ingrese número de nota'); return; }
    if (!this.formNota.conductor.trim()) { alert('Ingrese el conductor'); return; }

    const peticion = this.editandoNota && this.editandoNotaId
      ? this.trazabilidadService.editarNota(this.editandoNotaId, this.formNota)
      : this.trazabilidadService.agregarNota(this.registroSeleccionado.id, this.formNota);

    peticion.subscribe({
      next: () => { this.cargarNotas(this.registroSeleccionado.id); this.mostrarNota = false; },
      error: (err) => console.error(err)
    });
  }

  eliminarNota(id: number) {
    if (!confirm('¿Eliminar esta nota?')) return;
    this.trazabilidadService.eliminarNota(id).subscribe({
      next: () => this.cargarNotas(this.registroSeleccionado.id),
      error: (err) => console.error(err)
    });
  }

  cerrarModal() { this.mostrarModal = false; this.facturasDisponibles = []; }
  cerrarDetalle() { this.mostrarDetalle = false; this.registroSeleccionado = null; this.notas = []; }
  cerrarNota() { this.mostrarNota = false; }

  getBadgeEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente': return 'badge-pendiente';
      case 'EnTransito': return 'badge-transito';
      case 'Entregado': return 'badge-entregado';
      case 'Novedad': return 'badge-novedad';
      default: return 'badge-pendiente';
    }
  }

  getLabelEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente': return '⏳ Pendiente';
      case 'EnTransito': return '🚚 En tránsito';
      case 'Entregado': return '✅ Entregado';
      case 'Novedad': return '⚠️ Novedad';
      default: return estado;
    }
  }

  exportarExcel() {
    const datos = this.registrosFiltrados.map(r => ({
      'ID': r.id,
      'Fecha': new Date(r.fechaRegistro).toLocaleString(),
      'Factura/Remisión': r.facturaRemision,
      'Cliente': r.cliente,
      'Conductor': r.conductor,
      'Transportadora': r.transportadora || '-',
      'Vehículo': r.vehiculo || '-',
      'Guía': r.guia || '-',
      'Peso (kg)': r.pesoKilos ?? '-',
      'Valor flete': r.valorFlete ?? '-',
      'Factura entregada': r.facturaEntregada ? 'Sí' : 'No',
      'Acuse': r.ajusteRecibido ? 'Sí' : 'No',
      'Fecha entrega': r.fechaEntrega ? new Date(r.fechaEntrega).toLocaleString() : '-',
      'Estado': r.estado,
      'Novedad': r.novedad || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6 }, { wch: 20 }, { wch: 20 }, { wch: 22 }, { wch: 22 }, { wch: 20 },
      { wch: 12 }, { wch: 15 }, { wch: 10 }, { wch: 15 }, { wch: 15 }, { wch: 15 }, { wch: 20 }, { wch: 12 }, { wch: 30 }
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Trazabilidad');
    XLSX.writeFile(libro, `trazabilidad_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}