import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule }  from '@angular/forms';
import { HojaVidaService } from '../../services/hoja-vida.service';

@Component({
  selector: 'app-hoja-vida',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hoja-vida.html',
  styleUrls: ['./hoja-vida.scss']
})
export class HojaVidaComponent implements OnInit {

  @Input() conductorId!: number;

  conductor: any = null;
  tabActual = 'datos'; // datos | examenes | capacitaciones | infracciones
  cargando  = false;
  guardado  = false;

  // ── Modales ───────────────────────────────────────────────────────────────
  mostrarModalExamen       = false;
  mostrarModalCapacitacion = false;
  mostrarModalInfraccion   = false;

  // ── Formularios ───────────────────────────────────────────────────────────
  formDatos = {
    nombre:'', telefono:'', email:'', cedula:'',
    fechaNacimiento:'', direccion:'', tipoSangre:'',
    eps:'', arl:'', fondoPension:'',
    contactoEmergencia:'', telefonoEmergencia:'',
    licencia:'', categoriaLicencia:'',
    fechaVencimientoLicencia:'', fechaExpedicionLicencia:'',
    estado:'Activo'
  };

  formExamen = {
    tipoExamen:'', fechaExamen:'', fechaVencimiento:'',
    resultado:'', observaciones:'', medico:'', entidad:''
  };
  archivoExamen: File | null = null;

  formCapacitacion = {
    nombre:'', tipo:'', entidad:'', instructor:'',
    fechaInicio:'', fechaFin:'', duracionHoras: null as number | null,
    aprobado: true, observaciones:''
  };
  archivoCapacitacion: File | null = null;

  formInfraccion = {
    fechaInfraccion:'', tipoInfraccion:'', descripcion:'',
    lugar:'', valor: null as number | null,
    estado:'Pendiente', numeroComparendo:'', observaciones:''
  };
  archivoInfraccion: File | null = null;

  readonly tiposSangre     = ['A+','A-','B+','B-','AB+','AB-','O+','O-'];
  readonly categoriasLic   = ['A1','A2','B1','B2','B3','C1','C2','C3'];
  readonly estadosConductor= ['Activo','Inactivo','Suspendido','Vacaciones'];
  readonly tiposExamen     = ['Preingreso','Periódico','Retiro','Reintegro','Post-incapacidad'];
  readonly resultadosExamen= ['Apto','Apto con restricciones','No apto'];
  readonly tiposCapacitacion = ['Seguridad vial','Primeros auxilios','Manejo defensivo','Mercancías peligrosas','Fatiga','Normativa de tránsito','Otro'];
  readonly tiposInfraccion = ['Velocidad','Semáforo','Documentos','Alcoholemia','Estacionamiento','Carga','Otro'];
  readonly estadosInfraccion = ['Pendiente','Pagada','Apelada','Condonada'];

  constructor(private hojaVidaService: HojaVidaService) {}

  ngOnInit(): void { this.cargarHojaVida(); }

  cargarHojaVida() {
    this.cargando = true;
    this.hojaVidaService.obtenerHojaVida(this.conductorId).subscribe({
      next: (data) => {
        this.conductor = data;
        this.cargarFormDatos(data);
        this.cargando = false;
      },
      error: (err) => { console.error(err); this.cargando = false; }
    });
  }

  cargarFormDatos(c: any) {
    this.formDatos = {
      nombre:                    c.nombre               ?? '',
      telefono:                  c.telefono             ?? '',
      email:                     c.email                ?? '',
      cedula:                    c.cedula               ?? '',
      fechaNacimiento:           c.fechaNacimiento?.split('T')[0] ?? '',
      direccion:                 c.direccion            ?? '',
      tipoSangre:                c.tipoSangre           ?? '',
      eps:                       c.eps                  ?? '',
      arl:                       c.arl                  ?? '',
      fondoPension:              c.fondoPension         ?? '',
      contactoEmergencia:        c.contactoEmergencia   ?? '',
      telefonoEmergencia:        c.telefonoEmergencia   ?? '',
      licencia:                  c.licencia             ?? '',
      categoriaLicencia:         c.categoriaLicencia    ?? '',
      fechaVencimientoLicencia:  c.fechaVencimientoLicencia?.split('T')[0] ?? '',
      fechaExpedicionLicencia:   c.fechaExpedicionLicencia?.split('T')[0]  ?? '',
      estado:                    c.estado               ?? 'Activo'
    };
  }

  guardarDatos() {
    this.hojaVidaService.actualizarConductor(this.conductorId, this.formDatos).subscribe({
      next: () => { this.guardado = true; setTimeout(() => this.guardado = false, 3000); this.cargarHojaVida(); },
      error: (err) => { console.error(err); alert('Error guardando datos'); }
    });
  }

  // ── EXÁMENES ──────────────────────────────────────────────────────────────
  abrirModalExamen() {
    this.formExamen = { tipoExamen:'', fechaExamen:'', fechaVencimiento:'', resultado:'', observaciones:'', medico:'', entidad:'' };
    this.archivoExamen = null;
    this.mostrarModalExamen = true;
  }

  guardarExamen() {
    if (!this.formExamen.tipoExamen || !this.formExamen.fechaExamen || !this.formExamen.resultado) {
      alert('Complete los campos obligatorios'); return;
    }
    const fd = new FormData();
    Object.entries(this.formExamen).forEach(([k,v]) => { if (v) fd.append(this.capitalize(k), v as string); });
    if (this.archivoExamen) fd.append('Documento', this.archivoExamen);

    this.hojaVidaService.agregarExamen(this.conductorId, fd).subscribe({
      next: () => { this.mostrarModalExamen = false; this.cargarHojaVida(); },
      error: (err) => { console.error(err); alert('Error guardando examen'); }
    });
  }

  eliminarExamen(id: number) {
    if (!confirm('¿Eliminar examen?')) return;
    this.hojaVidaService.eliminarExamen(id).subscribe({ next: () => this.cargarHojaVida() });
  }

  // ── CAPACITACIONES ────────────────────────────────────────────────────────
  abrirModalCapacitacion() {
    this.formCapacitacion = { nombre:'', tipo:'', entidad:'', instructor:'', fechaInicio:'', fechaFin:'', duracionHoras:null, aprobado:true, observaciones:'' };
    this.archivoCapacitacion = null;
    this.mostrarModalCapacitacion = true;
  }

  guardarCapacitacion() {
    if (!this.formCapacitacion.nombre || !this.formCapacitacion.tipo || !this.formCapacitacion.fechaInicio) {
      alert('Complete los campos obligatorios'); return;
    }
    const fd = new FormData();
    Object.entries(this.formCapacitacion).forEach(([k,v]) => { if (v !== null && v !== '') fd.append(this.capitalize(k), String(v)); });
    if (this.archivoCapacitacion) fd.append('Documento', this.archivoCapacitacion);

    this.hojaVidaService.agregarCapacitacion(this.conductorId, fd).subscribe({
      next: () => { this.mostrarModalCapacitacion = false; this.cargarHojaVida(); },
      error: (err) => { console.error(err); alert('Error guardando capacitación'); }
    });
  }

  eliminarCapacitacion(id: number) {
    if (!confirm('¿Eliminar capacitación?')) return;
    this.hojaVidaService.eliminarCapacitacion(id).subscribe({ next: () => this.cargarHojaVida() });
  }

  // ── INFRACCIONES ──────────────────────────────────────────────────────────
  abrirModalInfraccion() {
    this.formInfraccion = { fechaInfraccion:'', tipoInfraccion:'', descripcion:'', lugar:'', valor:null, estado:'Pendiente', numeroComparendo:'', observaciones:'' };
    this.archivoInfraccion = null;
    this.mostrarModalInfraccion = true;
  }

  guardarInfraccion() {
    if (!this.formInfraccion.fechaInfraccion || !this.formInfraccion.tipoInfraccion) {
      alert('Complete los campos obligatorios'); return;
    }
    const fd = new FormData();
    Object.entries(this.formInfraccion).forEach(([k,v]) => { if (v !== null && v !== '') fd.append(this.capitalize(k), String(v)); });
    if (this.archivoInfraccion) fd.append('Documento', this.archivoInfraccion);

    this.hojaVidaService.agregarInfraccion(this.conductorId, fd).subscribe({
      next: () => { this.mostrarModalInfraccion = false; this.cargarHojaVida(); },
      error: (err) => { console.error(err); alert('Error guardando infracción'); }
    });
  }

  cambiarEstadoInfraccion(id: number, estado: string) {
    this.hojaVidaService.actualizarEstadoInfraccion(id, estado).subscribe({
      next: () => this.cargarHojaVida()
    });
  }

  eliminarInfraccion(id: number) {
    if (!confirm('¿Eliminar infracción?')) return;
    this.hojaVidaService.eliminarInfraccion(id).subscribe({ next: () => this.cargarHojaVida() });
  }

  // ── Utils ─────────────────────────────────────────────────────────────────
  seleccionarArchivo(event: any, tipo: string) {
    const file = event.target.files[0];
    if (!file) return;
    if (tipo === 'examen')       this.archivoExamen       = file;
    if (tipo === 'capacitacion') this.archivoCapacitacion = file;
    if (tipo === 'infraccion')   this.archivoInfraccion   = file;
  }

  private capitalize(s: string): string {
    return s.charAt(0).toUpperCase() + s.slice(1);
  }

  get edadConductor(): number {
    if (!this.conductor?.fechaNacimiento) return 0;
    const hoy   = new Date();
    const nac   = new Date(this.conductor.fechaNacimiento);
    let   edad  = hoy.getFullYear() - nac.getFullYear();
    const mes   = hoy.getMonth() - nac.getMonth();
    if (mes < 0 || (mes === 0 && hoy.getDate() < nac.getDate())) edad--;
    return edad;
  }

  licenciaVencida(fecha: string): boolean {
    if (!fecha) return false;
    return new Date(fecha) < new Date();
  }

  licenciaProxima(fecha: string): boolean {
    if (!fecha) return false;
    const venc  = new Date(fecha);
    const limit = new Date();
    limit.setDate(limit.getDate() + 60);
    return venc > new Date() && venc <= limit;
  }

  examenVencido(fecha: string): boolean {
    if (!fecha) return false;
    return new Date(fecha) < new Date();
  }
}