import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CambioRutaService } from '../../services/cambio-ruta.service';
import { ConductoresService } from '../../services/conductores.service';
import { VehiculosService } from '../../services/vehiculos.service';
import { AutorizacionesService } from '../../services/autorizaciones.service';
import { ContactosService } from '../../services/contactos.service';
import { ConfiguracionService } from '../../services/configuracion.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-cambio-ruta',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cambio-ruta.html',
  styleUrls: ['./cambio-ruta.scss']
})

export class CambioRutaComponent implements OnInit {

  cambios:        any[] = [];
  conductores:    any[] = [];
  vehiculos:      any[] = [];
  autorizaciones: any[] = [];

  mostrarModal  = false;
  mostrarAut    = false;
  seleccionado: any = null;
  accionAut     = '';

  filtroEstado  = '';
  nombreEmpresa = 'la empresa';

  form = {
    autorizacionId: null as number | null,
    conductorId:    0,
    vehiculoId:     0,
    rutaOriginal:   '',
    nuevaRuta:      '',
    motivoCambio:   ''
  };

  formAut = {
    autorizadoPor: '',
    observacion:   ''
  };

  constructor(
    private cambioRutaService:     CambioRutaService,
    private conductoresService:    ConductoresService,
    private vehiculosService:      VehiculosService,
    private autorizacionesService: AutorizacionesService,
    private contactosService:      ContactosService,
    private configuracionService:  ConfiguracionService
  ) {}

  ngOnInit(): void {
    this.cargarCambios();
    this.cargarConductores();
    this.cargarVehiculos();
    this.cargarAutorizaciones();

    this.configuracionService.obtenerConfiguracion().subscribe({
      next: (data: any) => {
        if (data.nombreEmpresa && data.nombreEmpresa.trim() !== '')
          this.nombreEmpresa = data.nombreEmpresa;
      },
      error: () => {}
    });
  }

  cargarCambios() {
    this.cambioRutaService.obtenerTodos().subscribe({
      next: (data) => this.cambios = data,
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

  cargarAutorizaciones() {
    this.autorizacionesService.obtenerAutorizaciones().subscribe({
      next: (data: any[]) => {
        this.autorizaciones = data.filter(a => a.estado === 'Autorizado');
      },
      error: (err) => console.error(err)
    });
  }

  get cambiosFiltrados(): any[] {
    return this.cambios.filter(c =>
      !this.filtroEstado || c.estado === this.filtroEstado
    );
  }

  get pendientes(): number {
    return this.cambios.filter(c => c.estado === 'Pendiente').length;
  }

  get autorizados(): number {
    return this.cambios.filter(c => c.estado === 'Autorizado').length;
  }

  get rechazados(): number {
    return this.cambios.filter(c => c.estado === 'Rechazado').length;
  }

  // IMPORTAR DESDE AUTORIZACIÓN
  importarAutorizacion(autorizacionId: number) {
    const aut = this.autorizaciones.find(a => a.id == autorizacionId);
    if (!aut) return;

    this.form.autorizacionId = aut.id;
    this.form.conductorId    = aut.conductor?.id   ?? 0;
    this.form.vehiculoId     = aut.vehiculo?.id    ?? 0;
    this.form.rutaOriginal   = aut.destinoCompleto ?? '';
  }

  nuevo() {
    this.form = {
      autorizacionId: null, conductorId: 0, vehiculoId: 0,
      rutaOriginal: '', nuevaRuta: '', motivoCambio: ''
    };
    this.mostrarModal = true;
  }

  guardar() {
    if (!this.form.conductorId)         { alert('Seleccione un conductor');  return; }
    if (!this.form.vehiculoId)          { alert('Seleccione un vehículo');   return; }
    if (!this.form.rutaOriginal.trim()) { alert('Ingrese la ruta original'); return; }
    if (!this.form.nuevaRuta.trim())    { alert('Ingrese la nueva ruta');    return; }
    if (!this.form.motivoCambio.trim()) { alert('Ingrese el motivo');        return; }

    this.cambioRutaService.crear(this.form).subscribe({
      next: (data) => {
        this.cargarCambios();
        this.cerrarModal();
        this.notificarContactos(data);
      },
      error: (err) => console.error(err)
    });
  }

  // NOTIFICAR POR WHATSAPP A CONTACTOS
  notificarContactos(cambio: any) {
    const conductor = this.conductores.find(c => c.id === this.form.conductorId);
    const vehiculo  = this.vehiculos.find(v => v.id  === this.form.vehiculoId);

    this.contactosService.obtenerContactos().subscribe({
      next: (contactos: any[]) => {
        const activos = contactos.filter(c => c.activo && c.recibeIncidentes);

        const mensaje = encodeURIComponent(
`🔄 *CAMBIO DE RUTA SOLICITADO* ⚠️
━━━━━━━━━━━━━━━━━━
👤 *Conductor:* ${conductor?.nombre ?? '-'}
🚗 *Vehículo:* ${vehiculo?.placa ?? '-'}
📍 *Ruta original:* ${this.form.rutaOriginal}
🆕 *Nueva ruta:* ${this.form.nuevaRuta}
❓ *Motivo:* ${this.form.motivoCambio}
📅 *Fecha:* ${new Date().toLocaleString()}
━━━━━━━━━━━━━━━━━━
⚠️ Requiere autorización de Bodega o Facturación
_${this.nombreEmpresa}_`
        );

        activos.forEach((contacto, index) => {
          setTimeout(() => {
            const numero = contacto.numeroWhatsApp.replace(/\D/g, '');
            window.open(`https://wa.me/${numero}?text=${mensaje}`, '_blank');
          }, index * 1000);
        });
      },
      error: (err) => console.error(err)
    });
  }

  // AUTORIZAR
  abrirAutorizar(cambio: any) {
    this.seleccionado = cambio;
    this.accionAut    = 'autorizar';
    this.formAut      = { autorizadoPor: '', observacion: '' };
    this.mostrarAut   = true;
  }

  // RECHAZAR
  abrirRechazar(cambio: any) {
    this.seleccionado = cambio;
    this.accionAut    = 'rechazar';
    this.formAut      = { autorizadoPor: '', observacion: '' };
    this.mostrarAut   = true;
  }

  confirmarAut() {
    if (!this.formAut.autorizadoPor.trim()) {
      alert('Ingrese su nombre'); return;
    }

    const accion$ = this.accionAut === 'autorizar'
      ? this.cambioRutaService.autorizar(this.seleccionado.id, this.formAut)
      : this.cambioRutaService.rechazar(this.seleccionado.id, this.formAut);

    accion$.subscribe({
      next: (data) => {
        this.cargarCambios();
        this.cerrarAut();

        if (this.accionAut === 'autorizar') {
          this.notificarConductor(data);
        }
      },
      error: (err) => console.error(err)
    });
  }

  notificarConductor(cambio: any) {
    const telefono = cambio.conductor?.telefono;
    if (!telefono) return;

    const mensaje = encodeURIComponent(
      `Hola ${cambio.conductor.nombre}, tu cambio de ruta ha sido AUTORIZADO ✅.\n` +
      `Nueva ruta: ${cambio.nuevaRuta}\n` +
      `Autorizado por: ${cambio.autorizadoPor}\n` +
      `- ${this.nombreEmpresa}`
    );

    const numero = telefono.replace(/\D/g, '');
    window.open(`https://wa.me/${numero}?text=${mensaje}`, '_blank');
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
      case 'Pendiente':  return 'badge-pendiente';
      case 'Autorizado': return 'badge-autorizado';
      case 'Rechazado':  return 'badge-rechazado';
      default:           return 'badge-pendiente';
    }
  }

  exportarExcel() {
    const datos = this.cambiosFiltrados.map(c => ({
      'ID':              c.id,
      'Fecha solicitud': new Date(c.fechaSolicitud).toLocaleString(),
      'Conductor':       c.conductor?.nombre  ?? '-',
      'Vehículo':        c.vehiculo?.placa    ?? '-',
      'Ruta original':   c.rutaOriginal,
      'Nueva ruta':      c.nuevaRuta,
      'Motivo':          c.motivoCambio,
      'Estado':          c.estado,
      'Autorizado por':  c.autorizadoPor      || '-',
      'Observación':     c.observacionAut     || '-',
      'Fecha aut.':      c.fechaAutorizacion
        ? new Date(c.fechaAutorizacion).toLocaleString() : '-',
    }));

    const hoja = XLSX.utils.json_to_sheet(datos);
    hoja['!cols'] = [
      { wch: 6  }, { wch: 20 }, { wch: 22 }, { wch: 12 },
      { wch: 30 }, { wch: 30 }, { wch: 30 }, { wch: 12 },
      { wch: 20 }, { wch: 30 }, { wch: 20 }
    ];

    const libro = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(libro, hoja, 'Cambios de Ruta');
    XLSX.writeFile(libro, `cambios_ruta_${new Date().toISOString().slice(0,10)}.xlsx`);
  }
}