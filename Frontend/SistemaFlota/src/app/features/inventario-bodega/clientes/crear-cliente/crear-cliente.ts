
import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { ClienteBodegaService } from '../../../../core/services/inventario-bodega/clientes/cliente-bodega.service';

import { ClienteBodega } from '../../../../core/models/inventario-bodega/clientes/cliente-bodega.model';

import {
  ClienteBodegaRequestDto,
  EditarClienteBodegaDto
} from '../../../../core/models/inventario-bodega/clientes/cliente-bodega.dto';

@Component({
  selector: 'app-crear-cliente',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './crear-cliente.html',
  styleUrl: './crear-cliente.scss'
})
export class CrearCliente implements OnInit {

  private clienteService = inject(ClienteBodegaService);
  private toastr = inject(ToastrService);

  private dialogRef = inject(MatDialogRef<CrearCliente>);

  data = inject(MAT_DIALOG_DATA) as {
    cliente?: ClienteBodega;
  };

  guardando = false;

  formulario: ClienteBodegaRequestDto = {
    nombre: '',
    direccion: null,
    telefono: null,
    email: null
  };

  get modoEdicion(): boolean {
    return !!this.data?.cliente;
  }

  ngOnInit(): void {

    if (this.data?.cliente) {

      this.formulario = {
        nombre: this.data.cliente.nombre,
        direccion: this.data.cliente.direccion,
        telefono: this.data.cliente.telefono,
        email: this.data.cliente.email
      };

    }
  }

  guardarCliente(): void {

    if (this.guardando) return;

    if (!this.formulario.nombre.trim()) {
      this.toastr.warning('El nombre del cliente es obligatorio.');
      return;
    }

    this.guardando = true;

    if (this.modoEdicion) {
      this.actualizarCliente();
    } else {
      this.crearCliente();
    }
  }

  // CREAR CLIENTE
  private crearCliente(): void {

    const dto: ClienteBodegaRequestDto = {
      nombre: this.formulario.nombre.trim(),
      direccion: this.formulario.direccion?.trim() || null,
      telefono: this.formulario.telefono?.trim() || null,
      email: this.formulario.email?.trim() || null
    };

    this.clienteService.crearCliente(dto).subscribe({

      next: respuesta => {

        this.guardando = false;

        if (!respuesta.exito) {
          this.toastr.error(respuesta.mensaje);
          return;
        }

        this.toastr.success(respuesta.mensaje);
        this.dialogRef.close(true);
      },

      error: error => {

        this.guardando = false;

        this.toastr.error(
          error.error?.mensaje || 'No fue posible crear el cliente.'
        );
      }
    });
  }

  // ACTUALIZAR CLIENTE
  private actualizarCliente(): void {

    const cliente = this.data?.cliente;

    if (!cliente) {
      this.guardando = false;
      return;
    }

    const dto: EditarClienteBodegaDto = {
      id: cliente.id,
      nombre: this.formulario.nombre.trim(),
      direccion: this.formulario.direccion?.trim() || null,
      telefono: this.formulario.telefono?.trim() || null,
      email: this.formulario.email?.trim() || null
    };

    this.clienteService.actualizarCliente(dto).subscribe({

      next: respuesta => {

        this.guardando = false;

        if (!respuesta.exito) {
          this.toastr.error(respuesta.mensaje);
          return;
        }

        this.toastr.success(respuesta.mensaje);
        this.dialogRef.close(true);
      },

      error: error => {

        this.guardando = false;

        this.toastr.error(
          error.error?.mensaje || 'No fue posible actualizar el cliente.'
        );
      }
    });
  }

  cerrarDialog(): void {

    if (this.guardando) return;

    this.dialogRef.close(false);
  }
}
