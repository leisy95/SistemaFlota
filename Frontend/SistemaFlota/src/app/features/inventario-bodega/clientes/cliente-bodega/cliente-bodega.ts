
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { ClienteBodegaService } from '../../../../core/services/inventario-bodega/clientes/cliente-bodega.service';

import {
  ClienteBodega as ClienteBodegaModel,
  ClienteBodegaFiltro,
  ClientesBodegaPaginados
} from '../../../../core/models/inventario-bodega/clientes/cliente-bodega.model';

import { CrearCliente } from '../crear-cliente/crear-cliente';
import { Dialog } from '../../../../shared/reutilizable/dialog/dialog';

@Component({
  selector: 'app-cliente-bodega',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './cliente-bodega.html',
  styleUrl: './cliente-bodega.scss',
})
export class ClienteBodega implements OnInit {

  private clienteService = inject(ClienteBodegaService);
  private toastr = inject(ToastrService);
  private dialog = inject(MatDialog);

  // LISTADO
  clientes: ClienteBodegaModel[] = [];

  cargando = false;

  // PAGINACIÓN
  pagina = 1;
  pageSize = 10;
  total = 0;
  totalPaginas = 0;

  // FILTROS
  buscar = '';
  filtroNombre = '';
  filtroTelefono = '';
  filtroEmail = '';

  ngOnInit(): void {
    this.listarClientes();
  }

  // LISTAR CLIENTES CON FILTROS Y PAGINACIÓN
  listarClientes(): void {

    this.cargando = true;

    const filtros: ClienteBodegaFiltro = {
      buscar: this.buscar,
      nombre: this.filtroNombre,
      telefono: this.filtroTelefono,
      email: this.filtroEmail,
      pagina: this.pagina,
      pageSize: this.pageSize
    };

    this.clienteService.listarClientes(filtros).subscribe({

      next: (respuesta: ClientesBodegaPaginados) => {

        this.clientes = respuesta.items ?? [];
        this.total = respuesta.total ?? 0;
        this.pagina = respuesta.pagina ?? 1;
        this.pageSize = respuesta.pageSize ?? 10;
        this.totalPaginas = respuesta.totalPaginas ?? 0;

        this.cargando = false;

      },

      error: (error) => {

        this.cargando = false;

        console.error('Error al listar clientes:', error);

        this.toastr.error(
          'No fue posible cargar los clientes.',
          'Error'
        );

      }

    });
  }

  // APLICAR FILTROS
  aplicarFiltros(): void {

    this.pagina = 1;
    this.listarClientes();

  }

  // LIMPIAR FILTROS
  limpiarFiltros(): void {

    this.buscar = '';
    this.filtroNombre = '';
    this.filtroTelefono = '';
    this.filtroEmail = '';

    this.pagina = 1;

    this.listarClientes();

  }

  // CAMBIAR PÁGINA
  cambiarPagina(nuevaPagina: number): void {

    if (
      nuevaPagina < 1 ||
      nuevaPagina > this.totalPaginas ||
      nuevaPagina === this.pagina ||
      this.cargando
    ) {
      return;
    }

    this.pagina = nuevaPagina;
    this.listarClientes();

  }

  // ABRIR FORMULARIO PARA CREAR CLIENTE
  abrirCrearCliente(): void {

    const dialogRef = this.dialog.open(CrearCliente, {
      width: '600px',
      maxWidth: '95vw',
      disableClose: true,
      data: {}
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (resultado === true) {

        this.pagina = 1;
        this.listarClientes();

      }

    });

  }

  // ABRIR FORMULARIO PARA EDITAR CLIENTE
  abrirEditarCliente(cliente: ClienteBodegaModel): void {

    const dialogRef = this.dialog.open(CrearCliente, {
      width: '600px',
      maxWidth: '95vw',
      disableClose: true,
      data: {
        cliente: cliente
      }
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (resultado === true) {
        this.listarClientes();
      }

    });

  }

  // DESACTIVAR CLIENTE
  // DESACTIVAR CLIENTE
  desactivarCliente(cliente: ClienteBodegaModel): void {

    if (!cliente.activo) {
      this.dialog.open(Dialog, {
        width: '400px',
        data: {
          titulo: 'Cliente desactivado',
          mensaje: 'El cliente ya se encuentra desactivado.',
          tipo: 'info',
          textoBoton: 'Aceptar',
          mostrarCancelar: false
        }
      });
      return;
    }

    const dialogRef = this.dialog.open(Dialog, {
      width: '400px',
      data: {
        titulo: '¿Desactivar cliente?',
        mensaje: `¿Deseas desactivar al cliente "${cliente.nombre}"?`,
        tipo: 'question',
        textoBoton: 'Sí, desactivar',
        textoCancelar: 'Cancelar',
        mostrarCancelar: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmado => {

      if (!confirmado) return;

      this.clienteService.desactivarCliente(cliente.id).subscribe({

        next: respuesta => {

          if (!respuesta.exito) {
            this.dialog.open(Dialog, {
              width: '400px',
              data: {
                titulo: 'No fue posible desactivar',
                mensaje: respuesta.mensaje,
                tipo: 'error'
              }
            });
            return;
          }

          this.dialog.open(Dialog, {
            width: '400px',
            data: {
              titulo: 'Cliente desactivado',
              mensaje: respuesta.mensaje,
              tipo: 'success'
            }
          });

          this.listarClientes();
        },

        error: error => {
          this.dialog.open(Dialog, {
            width: '400px',
            data: {
              titulo: 'Error',
              mensaje: error.error?.mensaje || 'No fue posible desactivar el cliente.',
              tipo: 'error'
            }
          });
        }
      });
    });
  }

  // REACTIVAR CLIENTE
  // REACTIVAR CLIENTE
  reactivarCliente(cliente: ClienteBodegaModel): void {

    if (cliente.activo) {
      this.dialog.open(Dialog, {
        width: '400px',
        data: {
          titulo: 'Cliente activo',
          mensaje: 'El cliente ya se encuentra activo.',
          tipo: 'info'
        }
      });
      return;
    }

    const dialogRef = this.dialog.open(Dialog, {
      width: '400px',
      data: {
        titulo: '¿Reactivar cliente?',
        mensaje: `¿Deseas reactivar al cliente "${cliente.nombre}"?`,
        tipo: 'question',
        textoBoton: 'Sí, reactivar',
        textoCancelar: 'Cancelar',
        mostrarCancelar: true
      }
    });

    dialogRef.afterClosed().subscribe(confirmado => {

      if (!confirmado) return;

      this.clienteService.reactivarCliente(cliente.id).subscribe({

        next: respuesta => {

          if (!respuesta.exito) {
            this.dialog.open(Dialog, {
              width: '400px',
              data: {
                titulo: 'No fue posible reactivar',
                mensaje: respuesta.mensaje,
                tipo: 'error'
              }
            });
            return;
          }

          this.dialog.open(Dialog, {
            width: '400px',
            data: {
              titulo: 'Cliente reactivado',
              mensaje: respuesta.mensaje,
              tipo: 'success'
            }
          });

          this.listarClientes();
        },

        error: error => {
          this.dialog.open(Dialog, {
            width: '400px',
            data: {
              titulo: 'Error',
              mensaje: error.error?.mensaje || 'No fue posible reactivar el cliente.',
              tipo: 'error'
            }
          });
        }
      });
    });
  }

}
