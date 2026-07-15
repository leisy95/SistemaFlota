import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CrearProveedor } from '../crear-proveedor/crear-proveedor';
import { FormsModule } from '@angular/forms';

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
export class ListarProveedores {

  buscar = '';
  proveedorSeleccionado = '';
  estado = '';
  orden = '';

  proveedores = [

    {
      nombre: 'Plásticos del Norte S.A.',
      nit: '900123456-7',
      contacto: 'Juan Pérez',
      telefono: '+57 1 234-5678',
      email: 'juan@plasticosnorte.com'
    },
    {
      nombre: 'Reciclados Premium Ltda',
      nit: '900234567-8',
      contacto: 'María González',
      telefono: '+57 1 234-5679',
      email: 'maria@recicladospremium.com'
    },
    {
      nombre: 'Polímeros Industriales S.A.S',
      nit: '900345678-9',
      contacto: 'Carlos Rodríguez',
      telefono: '+57 1 234-5680',
      email: 'carlos@polimeros.com'
    },
    {
      nombre: 'EcoPlast Colombia',
      nit: '900456789-0',
      contacto: 'Ana Martínez',
      telefono: '+57 1 234-5681',
      email: 'ana@ecoplast.com'
    },
    {
      nombre: 'Materiales Plásticos SRL',
      nit: '900567890-1',
      contacto: 'Roberto Silva',
      telefono: '+57 1 234-5682',
      email: 'roberto@materiales.com'
    }

  ];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog
  ) { }
  nuevoProveedor(): void {

    const dialogRef = this.dialog.open(CrearProveedor, {
      width: '700px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        // Recargar la lista o guardar el proveedor
      }
    });

  }

  editar(proveedor: any) {

    this.dialog.open(CrearProveedor, {
      width: '700px',
      data: proveedor
    });

  }

  eliminar(item: any) {
    this.toastr.error(
      item.nombre,
      'Proveedor eliminado'
    );
  }
}
