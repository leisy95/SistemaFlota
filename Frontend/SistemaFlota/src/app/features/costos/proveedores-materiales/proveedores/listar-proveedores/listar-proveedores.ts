import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CrearProveedor } from '../crear-proveedor/crear-proveedor';
import { FormsModule } from '@angular/forms';
import { Proveedor } from '../../../../../core/models/costos/proveedores/proveedores.model';
import { ProveedorService } from '../../../../../core/services/costos/proveedores/proveedor.service';

@Component({
  selector: 'app-listar-proveedores',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './listar-proveedores.html',
  styleUrl: './listar-proveedores.scss',
})
export class ListarProveedores implements OnInit {

  buscar = '';
  proveedorSeleccionado = '';
  estado = '';
  orden = '';

  pagina = 1;
  tamanoPagina = 10;

  totalRegistros = 0;
  totalPagina = 0;

  proveedores: Proveedor[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private proveedorService: ProveedorService,
  ) { }

  ngOnInit(): void {
    this.obtenerProveedores();
  }

  filtrar(): void {
    this.pagina = 1;
    this.obtenerProveedores();
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.estado = '';
    this.orden = '';
    this.proveedorSeleccionado = '';

    this.pagina = 1;

    this.obtenerProveedores();
  }

  obtenerProveedores(): void {

    this.proveedorService
      .obtener(
        this.buscar,
        this.estado,
        this.orden,
        this.pagina,
        this.tamanoPagina
      )
      .subscribe({
        next: (respuesta) => {
          this.proveedores = respuesta.datos;
          this.totalRegistros = respuesta.totalRegistros;
          this.totalPagina = respuesta.totalPaginas;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar los proveedores.',
            'Error'
          );
        }
      });
  }

  // Paginacion
  paginaAnterior(): void {
    if (this.pagina > 1) {
      this.pagina--;
      this.obtenerProveedores();
    }
  }

  paginaSiguiente(): void {
    if (this.pagina < this.totalPagina) {
      this.pagina++;
      this.obtenerProveedores();
    }
  }

  nuevoProveedor(): void {

    const dialogRef = this.dialog.open(CrearProveedor, {
      width: '700px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        this.obtenerProveedores();

        this.toastr.success(
          'Proveedor creado correctamente.',
          'Éxito'
        );
      }
    });

  }

  editar(item: Proveedor): void {

    this.proveedorService.obtenerPorId(item.idProveedor!).subscribe({
      next: (proveedor) => {
        const dialogRef = this.dialog.open(CrearProveedor, {
          width: '700px',
          disableClose: true,
          data: proveedor
        });

        dialogRef.afterClosed().subscribe(resultado => {
          if (resultado) {
            this.obtenerProveedores();
          }
        });
      },

      error: () => {
        this.toastr.error(
          'No fue posible cargar el proveedor.',
          'Error'
        );
      }
    });
  }

  eliminar(item: any) {
    this.toastr.error(
      item.nombre,
      'Proveedor eliminado'
    );
  }
}
