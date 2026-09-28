import { CommonModule } from '@angular/common';
import { Component, HostListener, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MaterialNoFormalizado } from '../../../../core/models/compras-no-formalizadas/materiales/material-no-formalizado.model';
import { environment } from '../../../../../environments/environment';
import { MaterialNoFormalizadoService, ProveedorNoFormalizadoFiltro } from '../../../../core/services/compras-no-formalizadas/materiales/material-no-formalizado.service';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { CrearMateriales } from '../crear-materiales/crear-materiales';

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
export class ListarMateriales implements OnInit {

  buscar = '';
  estado = '';
  orden = '';

  pagina = 1;
  tamanoPagina = 10;

  totalRegistros = 0;
  totalPagina = 0;

  materiales: MaterialNoFormalizado[] = [];

  documentoPdf?: string;
  fotosUrl = environment.fotosUrl;

  menuAbierto: number | null = null;

  materialSeleccionado: MaterialNoFormalizado | null = null;

  proveedor = '';
  proveedores: ProveedorNoFormalizadoFiltro[] = [];

  color = '';
  colores: string[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private materialService: MaterialNoFormalizadoService
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

  seleccionarMaterial(material: MaterialNoFormalizado): void {
    if (this.materialSeleccionado?.idMaterialNoFormalizado === material.idMaterialNoFormalizado) {
      this.materialSeleccionado = null;
    } else {
      this.materialSeleccionado = material;
    }
  }

  cerrarDrawer(): void {
    this.materialSeleccionado = null;
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
    ).subscribe({
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
    this.cerrarDrawer();
    this.obtenerMateriales();
  }

  paginaAnterior(): void {
    if (this.pagina > 1) {
      this.pagina--;
      this.cerrarDrawer();
      this.obtenerMateriales();
    }
  }

  paginaSiguiente(): void {
    if (this.pagina < this.totalPagina) {
      this.pagina++;
      this.cerrarDrawer();
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
        this.obtenerMateriales();
      }
    });
  }

  editar(item: MaterialNoFormalizado, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    this.menuAbierto = null;

    this.materialService.obtenerPorId(item.idMaterialNoFormalizado!).subscribe({
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

  eliminar(material: MaterialNoFormalizado, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    this.menuAbierto = null;
    console.log('Eliminar:', material);
  }

  obtenerUrlPdf(documentoPdf?: string): string {
    if (!documentoPdf) return '';
    return `${this.fotosUrl}/${documentoPdf}`;
  }
}
