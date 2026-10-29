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
import { Color } from '../../../../../core/models/costos/colores/color.model';
import { Categoria } from '../../../../../core/models/costos/categorias/categoria.models';
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
  materialInicial: Material | null = null;

  material: Material = {
    idProveedor: 0,
    codigo: '',
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
  colores: Color[] = [];
  categorias: Categoria[] = [];

  ngOnInit(): void {

    if (this.data) {
      this.material = { ...this.data };
      this.materialInicial = { ...this.data };
    }

    this.obtenerProveedores();
    this.obtenerColores();
    this.obtenerCategorias();
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

  obtenerColores(): void {
    this.materialService.obtenerColores().subscribe({
      next: (respuesta) => {
        console.log('COLORES API:', respuesta);
        console.log('COLOR MATERIAL:', this.material.color);

        this.colores = respuesta;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar los colores.',
          'Error'
        );
      }
    });
  }

  obtenerCategorias(): void {
    this.materialService.obtenerCategorias().subscribe({
      next: (respuesta) => {
        this.categorias = respuesta;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar las categorías.',
          'Error'
        );
      }
    });
  }

  guardar(): void {

    // EDITAR MATERIAL EXISTENTE
    if (this.data) {

      const id = this.material.idMaterial!;

      const hayCambios = this.hayCambiosDatos();

      // CASO 1: Solamente subir PDF
      if (!hayCambios && this.archivoPdf) {

        this.materialService.subirPdf(id, this.archivoPdf)
          .subscribe({

            next: () => {
              this.toastr.success(
                'PDF actualizado correctamente.',
                'Éxito'
              );

              this.dialogRef.close(true);
            },

            error: (error) => {
              this.toastr.error(
                error.error?.error || 'No fue posible subir el PDF.',
                'Error'
              );
            }
          });

        return;
      }

      // CASO 2: Actualizar datos y PDF
      if (hayCambios && this.archivoPdf) {

        const formData = this.crearFormData();

        this.materialService.actualizar(id, formData)
          .subscribe({

            next: () => {

              this.materialService.subirPdf(id, this.archivoPdf!)
                .subscribe({

                  next: () => {
                    this.toastr.success(
                      'Material y PDF actualizados correctamente.',
                      'Éxito'
                    );

                    this.dialogRef.close(true);
                  },

                  error: (error) => {
                    this.toastr.error(
                      error.error?.error ||
                      'Los datos se actualizaron, pero falló la carga del PDF.',
                      'Error'
                    );
                  }
                });

            },

            error: (error) => {
              this.toastr.error(
                error.error?.error || 'No fue posible actualizar el material.',
                'Error'
              );
            }
          });

        return;
      }

      // CASO 3: Solamente actualizar datos
      if (hayCambios) {

        const formData = this.crearFormData();

        this.materialService.actualizar(id, formData)
          .subscribe({

            next: () => {
              this.toastr.success(
                'Material actualizado correctamente.',
                'Éxito'
              );

              this.dialogRef.close(true);
            },

            error: (error) => {
              this.toastr.error(
                error.error?.error || 'No fue posible actualizar el material.',
                'Error'
              );
            }
          });

        return;
      }

      // CASO 4: Sin cambios
      this.toastr.info('No hay cambios para guardar.');
      return;
    }

    // CREAR MATERIAL NUEVO
    const formData = this.crearFormData();

    if (this.archivoPdf) {
      formData.append('archivoPdf', this.archivoPdf);
    }

    this.materialService.crear(formData).subscribe({

      next: (material) => {
        this.toastr.success(
          'Material creado correctamente.',
          'Éxito'
        );

        this.dialogRef.close(material);
      },

      error: (error) => {
        this.toastr.error(
          error.error?.error || 'No fue posible crear el material.',
          'Error'
        );
      }
    });
  }

  private hayCambiosDatos(): boolean {

    if (!this.materialInicial) return true;

    const campos = [
      'idProveedor',
      'codigo',
      'nombreMaterial',
      'descripcionCompra',
      'densidad',
      'categoria',
      'color',
      'tipoProduccion',
      'unidad',
      'precioBaseKg',
      'activo'
    ] as const;

    return campos.some(campo =>
      this.material[campo] !== this.materialInicial![campo]
    );
  }

  private crearFormData(): FormData {

    const formData = new FormData();

    formData.append('idProveedor', this.material.idProveedor.toString());
    formData.append('codigo', this.material.codigo);
    formData.append('nombreMaterial', this.material.nombreMaterial);
    formData.append('descripcionCompra', this.material.descripcionCompra ?? '');
    formData.append('densidad', this.material.densidad);
    formData.append('categoria', this.material.categoria);
    formData.append('color', this.material.color ?? '');
    formData.append('tipoProduccion', this.material.tipoProduccion ?? '');
    formData.append('unidad', this.material.unidad);
    formData.append('precioBaseKg', this.material.precioBaseKg.toString());
    formData.append('activo', this.material.activo.toString());

    return formData;
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