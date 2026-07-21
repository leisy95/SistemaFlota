import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CrearProveedor } from '../../proveedores/crear-proveedor/crear-proveedor';
import { Material } from '../../../../../core/models/materiales/material.models';
import { Proveedor } from '../../../../../core/models/costos/proveedores/proveedores.model';
import { MaterialService } from '../../../../../core/services/costos/materiales/materiales.service';
import { ProveedorService } from '../../../../../core/services/costos/proveedores/proveedor.service';
import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'app-crear-materiales',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    FormsModule
  ],
  templateUrl: './crear-materiales.html',
  styleUrl: './crear-materiales.scss',
})
export class CrearMateriales implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<CrearMateriales>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private materialService: MaterialService,
    private proveedorService: ProveedorService,
    private toastr: ToastrService


  ) { }

  material: Material = {
    idProveedor: 0,
    materiaPrima: '',
    descripcionCompra: '',
    densidad: '',
    categoria: '',
    color: '',
    lineaProduccion: '',
    unidad: '',
    precioBaseKg: 0,
    bultos: 0,
    cantidadKg: 0,
    activo: true
  };

  proveedores: Proveedor[] = [];

  ngOnInit(): void {
    this.obtenerProveedores();

    if (this.data) {
      this.material = { ...this.data };
    }
  }

  obtenerProveedores(): void {

    this.proveedorService
      .obtener('', '', 'nombre', 1, 1000)
      .subscribe({

        next: (respuesta) => {
          this.proveedores = respuesta.datos;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar los proveedores.',
            'Error'
          );
        }
      });
  }

  guardar(): void {

    if (this.data) {

      this.materialService.actualizar(
        this.material.idMaterial!,
        this.material
      ).subscribe({
        next: () => {
          this.toastr.success(
            'Material actualizado correctamente.',
            'Éxito'
          );
          this.dialogRef.close(true);
        },

        error: () => {
          this.toastr.error(
            'No fue posible actualizar el material.',
            'Error'
          );
        }
      });
      return;
    }


    this.materialService.crear(this.material).subscribe({
      next: () => {
        this.toastr.success(
          'Material creado correctamente.',
          'Éxito'
        );
        this.dialogRef.close(true);
      },

      error: () => {
        this.toastr.error(
          'No fue posible crear el material.',
          'Error'
        );
      }
    });

  }

  cerrar(): void {
    this.dialogRef.close();
  }
}