
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

}
