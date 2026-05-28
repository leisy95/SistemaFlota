import {
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ContactosService } from '../../services/contactos.service';

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
    nombre:           '',
    area:             '',
    numeroWhatsApp:   '',
    activo:           true,
    recibeIncidentes: true
  };

  readonly areas = [
    'Operaciones',
    'RRHH',
    'Gerencia',
    'Mantenimiento',
    'Logística',
    'Otro'
  ];

  constructor(
    private contactosService: ContactosService
  ) {}

  ngOnInit(): void {
    this.cargarContactos();
  }

  // =========================
  // CARGAR
  // =========================

  cargarContactos() {
    this.contactosService.obtenerContactos().subscribe({
      next: (data) => this.contactos = data,
      error: (err) => console.error(err)
    });
  }

  // =========================
  // NUEVO
  // =========================

  agregarContacto() {
    this.editando   = false;
    this.editandoId = null;
    this.nuevoContacto = {
      nombre: '', area: '', numeroWhatsApp: '',
      activo: true, recibeIncidentes: true
    };
    this.mostrarModal = true;
  }

  // =========================
  // GUARDAR
  // =========================

  guardarContacto() {
    if (!this.nuevoContacto.nombre)         { alert('Ingrese el nombre'); return; }
    if (!this.nuevoContacto.area)           { alert('Seleccione el área'); return; }
    if (!this.nuevoContacto.numeroWhatsApp) { alert('Ingrese el número de WhatsApp'); return; }

    // LIMPIAR NÚMERO — solo dígitos
    const numeroLimpio = this.nuevoContacto.numeroWhatsApp.replace(/\D/g, '');
    this.nuevoContacto.numeroWhatsApp = numeroLimpio;

    if (this.editando && this.editandoId) {
      this.contactosService
        .editarContacto(this.editandoId, this.nuevoContacto)
        .subscribe({
          next: () => { this.cargarContactos(); this.cerrarModal(); },
          error: (err) => console.error(err)
        });
    } else {
      this.contactosService
        .crearContacto(this.nuevoContacto)
        .subscribe({
          next: () => { this.cargarContactos(); this.cerrarModal(); },
          error: (err) => console.error(err)
        });
    }
  }

  // =========================
  // EDITAR
  // =========================

  editarContacto(contacto: any) {
    this.editando   = true;
    this.editandoId = contacto.id;
    this.nuevoContacto = {
      nombre:           contacto.nombre,
      area:             contacto.area,
      numeroWhatsApp:   contacto.numeroWhatsApp,
      activo:           contacto.activo,
      recibeIncidentes: contacto.recibeIncidentes
    };
    this.mostrarModal = true;
  }

  // =========================
  // ELIMINAR
  // =========================

  eliminarContacto(id: number) {
    if (!confirm('¿Eliminar contacto?')) return;
    this.contactosService.eliminarContacto(id).subscribe({
      next: () => this.cargarContactos(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // CAMBIAR ESTADO
  // =========================

  cambiarEstado(contacto: any) {
    this.contactosService.cambiarEstado(contacto.id).subscribe({
      next: () => this.cargarContactos(),
      error: (err) => console.error(err)
    });
  }

  // =========================
  // PROBAR WHATSAPP
  // =========================

  probarWhatsApp(contacto: any) {
    const mensaje = encodeURIComponent(
      `✅ Prueba de notificación del Sistema de Gestión de Flota. Este número está configurado para recibir alertas de incidentes en ruta.`
    );
    const url = `https://wa.me/${contacto.numeroWhatsApp}?text=${mensaje}`;
    window.open(url, '_blank');
  }

  // =========================
  // CERRAR
  // =========================

  cerrarModal() {
    this.mostrarModal = false;
  }

}