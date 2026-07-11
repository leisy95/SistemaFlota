import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as XLSX from 'xlsx';
import { PedidosService } from '../../../core/services/pedidos.service';
import { PermisosService } from '../../../core/services/permisos.service';

@Component({
  selector: 'app-pedidos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pedidos.html',
  styleUrls: ['./pedidos.scss']
})
export class PedidosComponent implements OnInit {

  pedidos: any[] = [];
  pedidosFiltrados: any[] = [];

  paginaActual = 1;
  porPagina = 10;
  totalRegistros = 0;
  totalPaginas = 0;

  mostrarModal = false;
  mostrarEstado = false;
  mostrarDetalle = false;
  editando = false;
  editandoId: number | null = null;
  seleccionado: any = null;
  rolUsuario = '';

  filtroEstado = '';
  filtroPrioridad = '';
  filtroBusqueda = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  form = {
    vendedorNombre: '',
    cliente: '',
    destino: '',
    prioridad: 'Normal',
    observaciones: ''
  };

  // ✅ Lista dinámica de referencias
  referencias: { referencia: string; cantidadKg: any; cantidadUnidades: any }[] = [];

  formEstado = {
    estado: '',
    gestionadoPor: ''
  };

  readonly prioridades = [
    { value: 'SOS', label: '🔴 SOS', class: 'prioridad-sos' },
    { value: 'Urgente', label: '🟡 Urgente', class: 'prioridad-urgente' },
    { value: 'Normal', label: '🟢 Normal', class: 'prioridad-normal' },
  ];

  readonly estadosList = [
    { value: 'Pendiente', label: '⏳ Pendiente' },
    { value: 'EnProceso', label: '🔄 En Proceso' },
    { value: 'Despachado', label: '🚚 Despachado' },
    { value: 'Entregado', label: '✅ Entregado' },
  ];

  get puedeCrear(): boolean { return this.permisosService.puedeCrear('pedidos'); }
  get puedeEditar(): boolean { return this.permisosService.puedeEditar('pedidos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('pedidos'); }

  get pendientes(): number { return this.pedidos.filter(p => p.estado === 'Pendiente').length; }
  get enProceso(): number { return this.pedidos.filter(p => p.estado === 'EnProceso').length; }
  get despachados(): number { return this.pedidos.filter(p => p.estado === 'Despachado').length; }
  get entregados(): number { return this.pedidos.filter(p => p.estado === 'Entregado').length; }
  get sos(): number { return this.pedidos.filter(p => p.prioridad === 'SOS').length; }

  constructor(
    private pedidosService: PedidosService,
    private permisosService: PermisosService
  ) { }

  ngOnInit(): void {
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (raw) {
      const user = JSON.parse(raw);
      this.rolUsuario = user.rol;
      this.form.vendedorNombre = user.username ?? '';
    }
    this.cargarPedidos();
  }

  cargarPedidos() {
    this.pedidosService.obtenerTodos({
      pagina: this.paginaActual,
      porPagina: this.porPagina,
      buscar: this.filtroBusqueda || undefined,
      estado: this.filtroEstado || undefined,
      prioridad: this.filtroPrioridad || undefined
    }).subscribe({
      next: (res: any) => {
        if (Array.isArray(res)) {
          this.pedidos = res;
          this.pedidosFiltrados = res;
          this.totalRegistros = res.length;
          this.totalPaginas = 1;
        } else {
          this.pedidos = res.data ?? [];
          this.pedidosFiltrados = res.data ?? [];
          this.totalRegistros = res.total ?? 0;
          this.totalPaginas = res.totalPaginas ?? 1;
        }
      },
      error: (err) => console.error(err)
    });
  }

  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.pedidosFiltrados = this.pedidos.filter(p => {
      const okEstado = !this.filtroEstado || p.estado === this.filtroEstado;
      const okPrioridad = !this.filtroPrioridad || p.prioridad === this.filtroPrioridad;
      const fecha = new Date(p.fechaRegistro);
      const okDesde = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq = !q ||
        p.cliente?.toLowerCase().includes(q) ||
        p.destino?.toLowerCase().includes(q) ||
        p.vendedorNombre?.toLowerCase().includes(q) ||
        p.referencias?.some((r: any) => r.referencia?.toLowerCase().includes(q));
      return okEstado && okPrioridad && okDesde && okHasta && okBusq;
      this.paginaActual = 1;
      this.cargarPedidos();
    });
  }

  limpiarFiltros() {
    this.filtroEstado = ''; this.filtroPrioridad = '';
    this.filtroBusqueda = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
    this.paginaActual = 1;
    this.cargarPedidos();
  }
  paginaSiguiente() {
    if (this.paginaActual < this.totalPaginas) {
      this.paginaActual++;
      this.cargarPedidos();
    }
  }

  paginaAnterior() {
    if (this.paginaActual > 1) {
      this.paginaActual--;
      this.cargarPedidos();
    }
  }


  // ✅ Agregar referencia
  agregarReferencia() {
    this.referencias = [...this.referencias, { referencia: '', cantidadKg: null, cantidadUnidades: null }];
  }

  // ✅ Eliminar referencia
  eliminarReferencia(i: number) {
    this.referencias = this.referencias.filter((_, idx) => idx !== i);
  }

  nuevo() {
    this.editando = false; this.editandoId = null;
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    const user = raw ? JSON.parse(raw) : null;
    this.form = {
      vendedorNombre: user?.username ?? '',
      cliente: '',
      destino: '',
      prioridad: 'Normal',
      observaciones: ''
    };
    this.referencias = [{ referencia: '', cantidadKg: null, cantidadUnidades: null }];
    this.mostrarModal = true;
  }

  editar(p: any) {
    this.editando = true; this.editandoId = p.id;
    this.form = {
      vendedorNombre: p.vendedorNombre,
      cliente: p.cliente,
      destino: p.destino,
      prioridad: p.prioridad,
      observaciones: p.observaciones ?? ''
    };
    this.referencias = p.referencias?.map((r: any) => ({
      referencia: r.referencia,
      cantidadKg: r.cantidadKg ?? null,
      cantidadUnidades: r.cantidadUnidades ?? null
    })) ?? [{ referencia: '', cantidadKg: null, cantidadUnidades: null }];
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.vendedorNombre.trim()) { alert('Ingrese el nombre del vendedor'); return; }
    if (!this.form.cliente.trim()) { alert('Ingrese el cliente'); return; }
    if (!this.form.destino.trim()) { alert('Ingrese el destino'); return; }
    if (this.referencias.length === 0) { alert('Agregue al menos una referencia'); return; }

    const refInvalida = this.referencias.find(r => !r.referencia.trim());
    if (refInvalida) { alert('Complete el nombre de todas las referencias'); return; }

    const dto = { ...this.form, referencias: this.referencias };

    const peticion = this.editando && this.editandoId
      ? this.pedidosService.editar(this.editandoId, dto)
      : this.pedidosService.crear(dto);

    peticion.subscribe({
      next: () => { this.cargarPedidos(); this.cerrarModal(); },
      error: (err) => console.error(err)
    });
  }

  abrirEstado(p: any) {
    this.seleccionado = p;
    this.formEstado = { estado: p.estado, gestionadoPor: '' };
    this.mostrarEstado = true;
  }

  guardarEstado() {
    if (!this.formEstado.estado) { alert('Seleccione un estado'); return; }
    if (!this.formEstado.gestionadoPor.trim()) { alert('Ingrese su nombre'); return; }

    this.pedidosService.cambiarEstado(this.seleccionado.id, this.formEstado).subscribe({
      next: () => { this.cargarPedidos(); this.cerrarEstado(); },
      error: (err) => console.error(err)
    });
  }

  verDetalle(p: any) { this.seleccionado = p; this.mostrarDetalle = true; }
  cerrarModal() { this.mostrarModal = false; this.referencias = []; }
  cerrarEstado() { this.mostrarEstado = false; this.seleccionado = null; }
  cerrarDetalle() { this.mostrarDetalle = false; this.seleccionado = null; }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este pedido?')) return;
    this.pedidosService.eliminar(id).subscribe({
      next: () => this.cargarPedidos(),
      error: (err) => console.error(err)
    });
  }

  getBadgePrioridad(p: string): string {
    switch (p) {
      case 'SOS': return 'badge-sos';
      case 'Urgente': return 'badge-urgente';
      default: return 'badge-normal';
    }
  }

  getBadgeEstado(e: string): string {
    switch (e) {
      case 'Pendiente': return 'badge-pendiente';
      case 'EnProceso': return 'badge-proceso';
      case 'Despachado': return 'badge-despachado';
      case 'Entregado': return 'badge-entregado';
      default: return 'badge-pendiente';
    }
  }

  getLabelEstado(e: string): string {
    switch (e) {
      case 'Pendiente': return '⏳ Pendiente';
      case 'EnProceso': return '🔄 En Proceso';
      case 'Despachado': return '🚚 Despachado';
      case 'Entregado': return '✅ Entregado';
      default: return e;
    }
  }

  getResumenReferencias(p: any): string {
    if (!p.referencias?.length) return '-';
    return p.referencias.map((r: any) => {
      const cant = [];
      if (r.cantidadKg) cant.push(`${r.cantidadKg} kg`);
      if (r.cantidadUnidades) cant.push(`${r.cantidadUnidades} uds`);
      return `${r.referencia}${cant.length ? ' (' + cant.join('/') + ')' : ''}`;
    }).join(', ');
  }

  exportarExcel() {
    const datos: any[] = [];
    this.pedidosFiltrados.forEach(p => {
      if (p.referencias?.length > 0) {
        p.referencias.forEach((r: any) => {
          datos.push({
            'ID': p.id,
            'Fecha': new Date(p.fechaRegistro).toLocaleString(),
            'Vendedor': p.vendedorNombre,
            'Cliente': p.cliente,
            'Destino': p.destino,
            'Referencia': r.referencia,
            'Kg': r.cantidadKg ?? '-',
            'Unidades': r.cantidadUnidades ?? '-',
            'Prioridad': p.prioridad,
            'Estado': p.estado,
            'Gestionado por': p.gestionadoPor || '-',
            'Observaciones': p.observaciones || '-',
          });
        });
      } else {
        datos.push({
          'ID': p.id,
          'Fecha': new Date(p.fechaRegistro).toLocaleString(),
          'Vendedor': p.vendedorNombre,
          'Cliente': p.cliente,
          'Destino': p.destino,
          'Referencia': '-',
          'Kg': '-',
          'Unidades': '-',
          'Prioridad': p.prioridad,
          'Estado': p.estado,
          'Gestionado por': p.gestionadoPor || '-',
          'Observaciones': p.observaciones || '-',
        });
      }
    });
    const hoja = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Pedidos');
    XLSX.writeFile(libro, `pedidos_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }
}
