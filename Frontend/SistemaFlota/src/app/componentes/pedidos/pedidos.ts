import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PedidosService } from '../../services/pedidos.service';
import { PermisosService } from '../../services/permisos.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-pedidos',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pedidos.html',
  styleUrls: ['./pedidos.scss']
})
export class PedidosComponent implements OnInit {

  pedidos:          any[] = [];
  pedidosFiltrados: any[] = [];

  mostrarModal   = false;
  mostrarEstado  = false;
  mostrarDetalle = false;
  editando       = false;
  editandoId:    number | null = null;
  seleccionado:  any = null;
  rolUsuario     = '';

  filtroEstado     = '';
  filtroPrioridad  = '';
  filtroBusqueda   = '';
  filtroFechaDesde = '';
  filtroFechaHasta = '';

  form = {
    vendedorNombre:   '',
    cliente:          '',
    referencia:       '',
    destino:          '',
    cantidadKg:       null as number | null,
    cantidadUnidades: null as number | null,
    prioridad:        'Normal',
    observaciones:    ''
  };

  formEstado = {
    estado:        '',
    gestionadoPor: ''
  };

  readonly prioridades = [
    { value: 'SOS',     label: '🔴 SOS',     class: 'prioridad-sos' },
    { value: 'Urgente', label: '🟡 Urgente',  class: 'prioridad-urgente' },
    { value: 'Normal',  label: '🟢 Normal',   class: 'prioridad-normal' },
  ];

  readonly estadosList = [
    { value: 'Pendiente',   label: '⏳ Pendiente'   },
    { value: 'EnProceso',   label: '🔄 En Proceso'  },
    { value: 'Despachado',  label: '🚚 Despachado'  },
    { value: 'Entregado',   label: '✅ Entregado'   },
  ];

  get puedeCrear():    boolean { return this.permisosService.puedeCrear('pedidos'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('pedidos'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('pedidos'); }

  get pendientes():  number { return this.pedidos.filter(p => p.estado === 'Pendiente').length; }
  get enProceso():   number { return this.pedidos.filter(p => p.estado === 'EnProceso').length; }
  get despachados(): number { return this.pedidos.filter(p => p.estado === 'Despachado').length; }
  get entregados():  number { return this.pedidos.filter(p => p.estado === 'Entregado').length; }
  get sos():         number { return this.pedidos.filter(p => p.prioridad === 'SOS').length; }

  constructor(
    private pedidosService: PedidosService,
    private permisosService: PermisosService
  ) {}

  ngOnInit(): void {
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (raw) {
      const user = JSON.parse(raw);
      this.rolUsuario = user.rol;
      // ✅ Autocompletar vendedor con nombre del usuario
      this.form.vendedorNombre = user.username ?? '';
    }
    this.cargarPedidos();
  }

  cargarPedidos() {
    this.pedidosService.obtenerTodos().subscribe({
      next: (data) => { this.pedidos = data; this.aplicarFiltros(); },
      error: (err)  => console.error(err)
    });
  }

  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.pedidosFiltrados = this.pedidos.filter(p => {
      const okEstado    = !this.filtroEstado    || p.estado    === this.filtroEstado;
      const okPrioridad = !this.filtroPrioridad || p.prioridad === this.filtroPrioridad;
      const fecha       = new Date(p.fechaRegistro);
      const okDesde     = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta     = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq      = !q ||
        p.cliente?.toLowerCase().includes(q)    ||
        p.referencia?.toLowerCase().includes(q) ||
        p.destino?.toLowerCase().includes(q)    ||
        p.vendedorNombre?.toLowerCase().includes(q);
      return okEstado && okPrioridad && okDesde && okHasta && okBusq;
    });
  }

  limpiarFiltros() {
    this.filtroEstado = ''; this.filtroPrioridad = '';
    this.filtroBusqueda = ''; this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  nuevo() {
    this.editando = false; this.editandoId = null;
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    const user = raw ? JSON.parse(raw) : null;
    this.form = {
      vendedorNombre:   user?.username ?? '',
      cliente:          '',
      referencia:       '',
      destino:          '',
      cantidadKg:       null,
      cantidadUnidades: null,
      prioridad:        'Normal',
      observaciones:    ''
    };
    this.mostrarModal = true;
  }

  editar(p: any) {
    this.editando = true; this.editandoId = p.id;
    this.form = {
      vendedorNombre:   p.vendedorNombre,
      cliente:          p.cliente,
      referencia:       p.referencia,
      destino:          p.destino,
      cantidadKg:       p.cantidadKg       ?? null,
      cantidadUnidades: p.cantidadUnidades ?? null,
      prioridad:        p.prioridad,
      observaciones:    p.observaciones    ?? ''
    };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.vendedorNombre.trim()) { alert('Ingrese el nombre del vendedor'); return; }
    if (!this.form.cliente.trim())        { alert('Ingrese el cliente');             return; }
    if (!this.form.referencia.trim())     { alert('Ingrese la referencia');          return; }
    if (!this.form.destino.trim())        { alert('Ingrese el destino');             return; }
    if (!this.form.cantidadKg && !this.form.cantidadUnidades) {
      alert('Ingrese al menos una cantidad (kg o unidades)'); return;
    }

    const peticion = this.editando && this.editandoId
      ? this.pedidosService.editar(this.editandoId, this.form)
      : this.pedidosService.crear(this.form);

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
    if (!this.formEstado.estado)        { alert('Seleccione un estado'); return; }
    if (!this.formEstado.gestionadoPor.trim()) { alert('Ingrese su nombre'); return; }

    this.pedidosService.cambiarEstado(this.seleccionado.id, this.formEstado).subscribe({
      next: () => { this.cargarPedidos(); this.cerrarEstado(); },
      error: (err) => console.error(err)
    });
  }

  verDetalle(p: any) { this.seleccionado = p; this.mostrarDetalle = true; }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este pedido?')) return;
    this.pedidosService.eliminar(id).subscribe({
      next: () => this.cargarPedidos(),
      error: (err) => console.error(err)
    });
  }

  cerrarModal()  { this.mostrarModal  = false; }
  cerrarEstado() { this.mostrarEstado = false; this.seleccionado = null; }
  cerrarDetalle(){ this.mostrarDetalle = false; this.seleccionado = null; }

  getBadgePrioridad(p: string): string {
    switch (p) {
      case 'SOS':     return 'badge-sos';
      case 'Urgente': return 'badge-urgente';
      case 'Normal':  return 'badge-normal';
      default:        return 'badge-normal';
    }
  }

  getBadgeEstado(e: string): string {
    switch (e) {
      case 'Pendiente':  return 'badge-pendiente';
      case 'EnProceso':  return 'badge-proceso';
      case 'Despachado': return 'badge-despachado';
      case 'Entregado':  return 'badge-entregado';
      default:           return 'badge-pendiente';
    }
  }

  getLabelEstado(e: string): string {
    switch (e) {
      case 'Pendiente':  return '⏳ Pendiente';
      case 'EnProceso':  return '🔄 En Proceso';
      case 'Despachado': return '🚚 Despachado';
      case 'Entregado':  return '✅ Entregado';
      default:           return e;
    }
  }

  getCantidad(p: any): string {
    const partes = [];
    if (p.cantidadKg)       partes.push(`${p.cantidadKg} kg`);
    if (p.cantidadUnidades) partes.push(`${p.cantidadUnidades} uds`);
    return partes.join(' / ') || '-';
  }

  exportarExcel() {
    const datos = this.pedidosFiltrados.map(p => ({
      'ID':           p.id,
      'Fecha':        new Date(p.fechaRegistro).toLocaleString(),
      'Vendedor':     p.vendedorNombre,
      'Cliente':      p.cliente,
      'Referencia':   p.referencia,
      'Destino':      p.destino,
      'Kg':           p.cantidadKg       ?? '-',
      'Unidades':     p.cantidadUnidades ?? '-',
      'Prioridad':    p.prioridad,
      'Estado':       p.estado,
      'Gestionado por': p.gestionadoPor  || '-',
      'Observaciones':  p.observaciones  || '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Pedidos');
    XLSX.writeFile(libro, `pedidos_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}