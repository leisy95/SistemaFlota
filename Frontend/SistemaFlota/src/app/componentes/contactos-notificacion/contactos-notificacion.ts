import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactosService } from '../../services/contactos.service';
import { PermisosService }  from '../../services/permisos.service';

@Component({
  selector: 'app-contactos-notificacion',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contactos-notificacion.html',
  styleUrls: ['./contactos-notificacion.scss']
})
export class ContactosNotificacionComponent implements OnInit {

  contactos:    any[] = [];
  mostrarModal  = false;
  editando      = false;
  editandoId:   number | null = null;

  nuevoContacto = {
    nombre: '', area: '', numeroWhatsApp: '',
    activo: true, recibeIncidentes: true
  };

  readonly areas = ['Operaciones','RRHH','Gerencia','Mantenimiento','Logística','Otro'];

  // ── Permisos ─────────────────────────────────────────────────────────────────
  get puedeCrear():    boolean { return this.permisosService.puedeCrear('contactos-notificacion'); }
  get puedeEditar():   boolean { return this.permisosService.puedeEditar('contactos-notificacion'); }
  get puedeEliminar(): boolean { return this.permisosService.puedeEliminar('contactos-notificacion'); }

  constructor(
    private contactosService: ContactosService,
    private permisosService:  PermisosService
  ) {}

  ngOnInit(): void { this.cargarContactos(); }

  cargarContactos() {
    this.contactosService.obtenerContactos().subscribe({
      next: (data) => this.contactos = data,
      error: (err)  => console.error(err)
    });
  }

  agregarContacto() {
    this.editando = false; this.editandoId = null;
    this.nuevoContacto = { nombre: '', area: '', numeroWhatsApp: '', activo: true, recibeIncidentes: true };
    this.mostrarModal = true;
  }

  guardarContacto() {
    if (!this.nuevoContacto.nombre)         { alert('Ingrese el nombre');            return; }
    if (!this.nuevoContacto.area)           { alert('Seleccione el área');           return; }
    if (!this.nuevoContacto.numeroWhatsApp) { alert('Ingrese el número WhatsApp');   return; }

    this.nuevoContacto.numeroWhatsApp = this.nuevoContacto.numeroWhatsApp.replace(/\D/g, '');

    const peticion = this.editando && this.editandoId
      ? this.contactosService.editarContacto(this.editandoId, this.nuevoContacto)
      : this.contactosService.crearContacto(this.nuevoContacto);

    peticion.subscribe({
      next: () => { this.cargarContactos(); this.cerrarModal(); },
      error: (err) => console.error(err)
    });
  }

  editarContacto(contacto: any) {
    this.editando = true; this.editandoId = contacto.id;
    this.nuevoContacto = {
      nombre:           contacto.nombre,
      area:             contacto.area,
      numeroWhatsApp:   contacto.numeroWhatsApp,
      activo:           contacto.activo,
      recibeIncidentes: contacto.recibeIncidentes
    };
    this.mostrarModal = true;
  }

  eliminarContacto(id: number) {
    if (!confirm('¿Eliminar contacto?')) return;
    this.contactosService.eliminarContacto(id).subscribe({
      next: () => this.cargarContactos(),
      error: (err) => console.error(err)
    });
  }

  cambiarEstado(contacto: any) {
    this.contactosService.cambiarEstado(contacto.id).subscribe({
      next: () => this.cargarContactos(),
      error: (err) => console.error(err)
    });
  }

  probarWhatsApp(contacto: any) {
    const mensaje = encodeURIComponent(
      `✅ Prueba de notificación del Sistema de Gestión de Flota. Este número está configurado para recibir alertas de incidentes en ruta.`
    );
    window.open(`https://wa.me/${contacto.numeroWhatsApp}?text=${mensaje}`, '_blank');
  }

  cerrarModal() { this.mostrarModal = false; }
}