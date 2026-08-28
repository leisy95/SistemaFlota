import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CrearProveedor } from '../../proveedores/crear-proveedor/crear-proveedor';
import { Material } from '../../../../../core/models/costos/materiales/material.models';
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

  archivoPdf: File | null = null;
  nombreArchivo = '';

  material: Material = {
    idProveedor: 0,
    nombreMaterial: '',
    descripcionCompra: '',
    densidad: '',
    categoria: '',
    color: '',
    tipoProduccion: '',
    unidad: '',
    precioBaseKg: 0,
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

    const formData = new FormData();

    formData.append('idProveedor', this.material.idProveedor.toString());
    formData.append('nombreMaterial', this.material.nombreMaterial);
    formData.append('descripcionCompra', this.material.descripcionCompra ?? '');
    formData.append('densidad', this.material.densidad);
    formData.append('categoria', this.material.categoria);
    formData.append('color', this.material.color ?? '');
    formData.append('tipoProduccion', this.material.tipoProduccion ?? '');
    formData.append('unidad', this.material.unidad);
    formData.append('precioBaseKg', this.material.precioBaseKg.toString());
    formData.append('activo', this.material.activo.toString());

    if (this.archivoPdf) {
      formData.append('archivoPdf', this.archivoPdf);
    }

    if (this.data) {

      this.materialService.actualizar(
        this.material.idMaterial!,
        formData
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

    this.materialService.crear(formData).subscribe({
      next: (material) => {
        this.toastr.success(
          'Material creado correctamente.',
          'Éxito'
        );
        this.dialogRef.close(material);
      },

      error: () => {
        this.toastr.error(
          'No fue posible crear el material.',
          'Error'
        );
      }
    });

  }

  seleccionarArchivo(event: Event): void {

    const input = event.target as HTMLInputElement;

    if (!input.files?.length)
      return;

    const archivo = input.files[0];

    if (archivo.type !== 'application/pdf') {
      this.toastr.warning('Solo se permiten archivos PDF');
      return;
    }

    this.archivoPdf = archivo;
    this.nombreArchivo = archivo.name;
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}