import { Component, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { CrearMateriales } from '../crear-materiales/crear-materiales';
import { Material } from '../../../../../core/models/costos/materiales/material.models';
import { MaterialService } from '../../../../../core/services/costos/materiales/materiales.service';
import { ProveedorFiltro } from '../../../../../core/models/costos/materiales/filtros-material.models';

@Component({
  selector: 'app-listar-materiales',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './listar-materiales.html',
  styleUrl: './listar-materiales.scss',
})
export class ListarMateriales {

  buscar = '';
  estado = '';
  orden = '';

  pagina = 1;
  tamanoPagina = 10;

  totalRegistros = 0;
  totalPagina = 0;
  materiales: Material[] = [];

  documentoPdf?: string;
  menuAbierto: number | null = null;

  // filtros
  proveedor = '';
  proveedores: ProveedorFiltro[] = [];

  color = '';
  colores: string[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private materialService: MaterialService,

  ) { }

  ngOnInit(): void {

    this.obtenerMateriales();
    this.cargarFiltros();
  }

  @HostListener('document:click', ['$event'])
  clickFuera(event: MouseEvent): void {

    const target = event.target as HTMLElement;

    if (!target.closest('.menu-container')) {
      this.menuAbierto = null;
    }
  }

  toggleMenu(id: number): void {
    this.menuAbierto = this.menuAbierto === id ? null : id;
  }

  cargarFiltros(): void {

    this.materialService.obtenerFiltros().subscribe({

      next: (respuesta) => {
        this.proveedores = respuesta.proveedores;
        this.colores = respuesta.colores;
      },

      error: () => {
        this.toastr.error(
          'No fue posible cargar los filtros',
          'Error'
        );
      }

    });

  }

  obtenerMateriales(): void {

    this.materialService.obtener(
      this.buscar,
      this.estado,
      this.orden,
      this.proveedor,
      this.color,
      this.pagina,
      this.tamanoPagina
    )
      .subscribe({

        next: (respuesta) => {
          this.materiales = respuesta.datos;
          this.totalRegistros = respuesta.totalRegistros;
          this.totalPagina = respuesta.totalPaginas;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar los materiales',
            'Error'
          );
        }
      });
  }

  filtrar(): void {
    this.pagina = 1;
    this.obtenerMateriales();
  }

  paginaAnterior(): void {
    if (this.pagina > 1) {
      this.pagina--;
      this.obtenerMateriales();
    }
  }

  paginaSiguiente(): void {
    if (this.pagina < this.totalPagina) {
      this.pagina++;
      this.obtenerMateriales();
    }
  }

  nuevoMaterial(): void {
    const dialogRef = this.dialog.open(CrearMateriales, {
      width: '700px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        // Recargar la lista o guardar el proveedor
      }
    });
  }

  editar(item: Material): void {

    this.menuAbierto = null;

    this.materialService.obtenerPorId(item.idMaterial!).subscribe({

      next: (material) => {

        const dialogRef = this.dialog.open(CrearMateriales, {
          width: '700px',
          disableClose: true,
          data: material
        });

        dialogRef.afterClosed().subscribe(resultado => {

          if (resultado) {
            this.obtenerMateriales();
          }
        });
      },

      error: () => {
        this.toastr.error(
          'No fue posible cargar el material.',
          'Error'
        );
      }
    });

  }

  cerrarMenu(): void {
    this.menuAbierto = null;
  }

  eliminar(material: any): void {
    this.menuAbierto = null;
    console.log('Eliminar:', material);
  }

}