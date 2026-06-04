import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CambioRutaService }     from '../../services/cambio-ruta.service';
import { ConductoresService }    from '../../services/conductores.service';
import { VehiculosService }      from '../../services/vehiculos.service';
import { AutorizacionesService } from '../../services/autorizaciones.service';
import { PermisosService }       from '../../services/permisos.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-cambio-ruta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cambio-ruta.html',
  styleUrls: ['./cambio-ruta.scss']
})
export class CambioRutaComponent implements OnInit {

  cambios:           any[] = [];
  cambiosFiltrados:  any[] = [];
  conductores:       any[] = [];
  vehiculos:         any[] = [];
  autorizaciones:    any[] = [];

  mostrarModal  = false;
  mostrarAut    = false;
  seleccionado: any = null;
  accionAut     = '';
  rolUsuario    = '';

  filtroEstado      = '';
  filtroBusqueda    = '';
  filtroFechaDesde  = '';
  filtroFechaHasta  = '';

  form = {
    autorizacionId: null as number | null,
    conductorId:    0,
    vehiculoId:     0,
    rutaOriginal:   '',
    nuevaRuta:      '',
    motivoCambio:   ''
  };

  formAut = { autorizadoPor: '', observacion: '' };

  get puedeCrear():    boolean { return this.permisosService.puedeCrear('cambio-ruta'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('cambio-ruta'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('cambio-ruta'); }

  get pendientes():   number { return this.cambios.filter(c => c.estado === 'Pendiente').length; }
  get autorizados():  number { return this.cambios.filter(c => c.estado === 'Autorizado').length; }
  get rechazados():   number { return this.cambios.filter(c => c.estado === 'Rechazado').length; }
  get confirmados():  number { return this.cambios.filter(c => c.estado === 'Confirmado').length; }

  constructor(
    private cambioRutaService:     CambioRutaService,
    private conductoresService:    ConductoresService,
    private vehiculosService:      VehiculosService,
    private autorizacionesService: AutorizacionesService,
    private permisosService:       PermisosService
  ) {}

  ngOnInit(): void {
    const raw = sessionStorage.getItem('user') || localStorage.getItem('user');
    if (raw) this.rolUsuario = JSON.parse(raw).rol;
    this.cargarCambios();
    this.cargarConductores();
    this.cargarVehiculos();
    this.cargarAutorizaciones();
  }

  cargarCambios() {
    this.cambioRutaService.obtenerTodos().subscribe({
      next: (data) => { this.cambios = data; this.aplicarFiltros(); },
      error: (err)  => console.error(err)
    });
  }

  cargarConductores() {
    this.conductoresService.obtenerConductores().subscribe({
      next: (data) => this.conductores = data,
      error: (err)  => console.error(err)
    });
  }

  cargarVehiculos() {
    this.vehiculosService.obtenerVehiculos().subscribe({
      next: (data) => this.vehiculos = data,
      error: (err)  => console.error(err)
    });
  }

  cargarAutorizaciones() {
    this.autorizacionesService.obtenerAutorizaciones().subscribe({
      next: (data: any[]) => this.autorizaciones = data.filter(a => a.estado === 'Autorizado'),
      error: (err) => console.error(err)
    });
  }

  aplicarFiltros() {
    const q = this.filtroBusqueda.toLowerCase();
    this.cambiosFiltrados = this.cambios.filter(c => {
      const okEstado = !this.filtroEstado || c.estado === this.filtroEstado;
      const fecha    = new Date(c.fechaSolicitud);
      const okDesde  = !this.filtroFechaDesde || fecha >= new Date(this.filtroFechaDesde);
      const okHasta  = !this.filtroFechaHasta || fecha <= new Date(this.filtroFechaHasta + 'T23:59:59');
      const okBusq   = !q ||
        c.conductor?.nombre?.toLowerCase().includes(q) ||
        c.vehiculo?.placa?.toLowerCase().includes(q)   ||
        c.rutaOriginal?.toLowerCase().includes(q)      ||
        c.nuevaRuta?.toLowerCase().includes(q)         ||
        c.motivoCambio?.toLowerCase().includes(q);
      return okEstado && okDesde && okHasta && okBusq;
    });
  }

  limpiarFiltros() {
    this.filtroEstado = ''; this.filtroBusqueda = '';
    this.filtroFechaDesde = ''; this.filtroFechaHasta = '';
    this.aplicarFiltros();
  }

  importarAutorizacion(autorizacionId: number) {
    const aut = this.autorizaciones.find(a => a.id == autorizacionId);
    if (!aut) return;
    this.form.autorizacionId = aut.id;
    this.form.conductorId    = aut.conductor?.id   ?? 0;
    this.form.vehiculoId     = aut.vehiculo?.id    ?? 0;
    this.form.rutaOriginal   = aut.destinoCompleto ?? '';
  }

  nuevo() {
    this.form = { autorizacionId: null, conductorId: 0, vehiculoId: 0, rutaOriginal: '', nuevaRuta: '', motivoCambio: '' };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.conductorId)         { alert('Seleccione un conductor');  return; }
    if (!this.form.vehiculoId)          { alert('Seleccione un vehículo');   return; }
    if (!this.form.rutaOriginal.trim()) { alert('Ingrese la ruta original'); return; }
    if (!this.form.nuevaRuta.trim())    { alert('Ingrese la nueva ruta');    return; }
    if (!this.form.motivoCambio.trim()) { alert('Ingrese el motivo');        return; }

    this.cambioRutaService.crear(this.form).subscribe({
      next: () => { this.cargarCambios(); this.cerrarModal(); },
      error: (err) => console.error(err)
    });
  }

  abrirAutorizar(cambio: any) {
    this.seleccionado = cambio; this.accionAut = 'autorizar';
    this.formAut = { autorizadoPor: '', observacion: '' };
    this.mostrarAut = true;
  }

  abrirRechazar(cambio: any) {
    this.seleccionado = cambio; this.accionAut = 'rechazar';
    this.formAut = { autorizadoPor: '', observacion: '' };
    this.mostrarAut = true;
  }

  confirmarAut() {
    if (!this.formAut.autorizadoPor.trim()) { alert('Ingrese su nombre'); return; }
    const accion$ = this.accionAut === 'autorizar'
      ? this.cambioRutaService.autorizar(this.seleccionado.id, this.formAut)
      : this.cambioRutaService.rechazar(this.seleccionado.id, this.formAut);

    accion$.subscribe({
      next: () => { this.cargarCambios(); this.cerrarAut(); },
      error: (err) => console.error(err)
    });
  }

  // ✅ Conductor confirma que recibió la autorización
  confirmarCambio(cambio: any) {
    if (!confirm('¿Confirmar que recibiste la autorización del cambio de ruta?')) return;
    this.cambioRutaService.confirmar(cambio.id).subscribe({
      next: () => { this.cargarCambios(); },
      error: (err) => console.error(err)
    });
  }

  eliminar(id: number) {
    if (!confirm('¿Eliminar este registro?')) return;
    this.cambioRutaService.eliminar(id).subscribe({
      next: () => this.cargarCambios(),
      error: (err) => console.error(err)
    });
  }

  cerrarModal() { this.mostrarModal = false; }
  cerrarAut()   { this.mostrarAut = false; this.seleccionado = null; }

  getBadgeEstado(estado: string): string {
    switch (estado) {
      case 'Pendiente':   return 'badge-pendiente';
      case 'Autorizado':  return 'badge-autorizado';
      case 'Confirmado':  return 'badge-confirmado';
      case 'Rechazado':   return 'badge-rechazado';
      default:            return 'badge-pendiente';
    }
  }

  exportarExcel() {
    const datos = this.cambiosFiltrados.map(c => ({
      'ID':              c.id,
      'Fecha solicitud': new Date(c.fechaSolicitud).toLocaleString(),
      'Conductor':       c.conductor?.nombre ?? '-',
      'Vehículo':        c.vehiculo?.placa   ?? '-',
      'Ruta original':   c.rutaOriginal,
      'Nueva ruta':      c.nuevaRuta,
      'Motivo':          c.motivoCambio,
      'Estado':          c.estado,
      'Autorizado por':  c.autorizadoPor     || '-',
      'Observación':     c.observacionAut    || '-',
      'Fecha aut.':      c.fechaAutorizacion ? new Date(c.fechaAutorizacion).toLocaleString() : '-',
    }));
    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      {wch:6},{wch:20},{wch:22},{wch:12},{wch:30},{wch:30},{wch:30},{wch:12},{wch:20},{wch:30},{wch:20}
    ];
    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Cambios de Ruta');
    XLSX.writeFile(libro, `cambios_ruta_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}