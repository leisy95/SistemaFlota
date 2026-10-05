import { Component, OnInit } from '@angular/core';
import { ProveedorNoFormalizado } from '../../../../core/models/compras-no-formalizadas/proveedores/proveedor-no-formalizado.model';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { ProveedorNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/proveedores/proveedor-no-formalizado.service';
import { CrearProveedor } from '../crear-proveedor/crear-proveedor';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-listar-proveedores',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './listar-proveedores.html',
  styleUrl: './listar-proveedores.scss',
})
export class ListarProveedores implements OnInit {

  buscar = '';
  estado = '';
  orden = '';

  pagina = 1;
  tamanoPagina = 10;

  totalRegistros = 0;
  totalPagina = 0;

  proveedores: ProveedorNoFormalizado[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private proveedorService: ProveedorNoFormalizadoService
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
    this.pagina = 1;
    this.obtenerProveedores();
  }

  obtenerProveedores(): void {
    this.proveedorService.obtener(
      this.buscar,
      this.estado,
      this.orden,
      this.pagina,
      this.tamanoPagina
    ).subscribe({
      next: (respuesta) => {
        this.proveedores = respuesta.datos;
        this.totalRegistros = respuesta.totalRegistros;
        this.totalPagina = respuesta.totalPaginas;
      },
      error: () => {
        this.toastr.error('No fue posible cargar los proveedores no formalizados.', 'Error');
      }
    });
  }

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
        this.toastr.success('Proveedor no formalizado creado correctamente.', 'Éxito');
      }
    });
  }

  editar(item: ProveedorNoFormalizado): void {
    if (!item.idProveedorNoFormalizado) {
      return;
    }

    this.proveedorService.obtenerPorId(item.idProveedorNoFormalizado).subscribe({
      next: (proveedor) => {
        const dialogRef = this.dialog.open(CrearProveedor, {
          width: '700px',
          disableClose: true,
          data: proveedor
        });

        dialogRef.afterClosed().subscribe(resultado => {
          if (resultado) {
            this.obtenerProveedores();
            this.toastr.success('Proveedor no formalizado actualizado correctamente.', 'Éxito');
          }
        });
      },
      error: () => {
        this.toastr.error('No fue posible cargar el proveedor no formalizado.', 'Error');
      }
    });
  }

  eliminar(item: ProveedorNoFormalizado): void {
    this.toastr.error(item.nombre, 'Proveedor eliminado');
  }
}