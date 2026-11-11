
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';

import { InventarioBodega } from '../../../../core/models/inventario-bodega/inventario-bodega.models';
import { InventarioBodegaService } from '../../../../core/services/inventario-bodega/Inventario/inventario-bodega.service';
import { CrearInventario } from '../crear-inventario/crear-inventario';
import { DetalleInvenatrio } from '../detalle-invenatrio/detalle-invenatrio';

interface ResumenInventario {
  totalProductos: number;
  pesoTotal: number;
  cantidadTotal: number;
  totalColores: number;
  totalMateriales: number;
  totalReferencias: number;
}

interface FiltrosInventario {
  materiales: string[];
  colores: string[];
}

interface InventarioPaginadoResponse {
  resumen: ResumenInventario;
  filtros: FiltrosInventario;
  items: InventarioBodega[];
  total: number;
  pagina: number;
  pageSize: number;
  totalPaginas: number;
}

@Component({
  selector: 'app-listar-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './listar-inventario.html',
  styleUrl: './listar-inventario.scss',
})
export class ListarInventario implements OnInit {

  buscar = '';
  materialFiltro = '';
  colorFiltro = '';

  pagina = 1;
  pageSize = 5;

  cargando = false;
  cargandoInventario = false;

  inventario: InventarioBodega[] = [];

  resumen: ResumenInventario = {
    totalProductos: 0,
    pesoTotal: 0,
    cantidadTotal: 0,
    totalColores: 0,
    totalMateriales: 0,
    totalReferencias: 0
  };

  filtros: FiltrosInventario = {
    materiales: [],
    colores: []
  };

  totalFiltrado = 0;
  totalPaginas = 1;

  constructor(
    private inventarioService: InventarioBodegaService,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.cargarInventario();
  }

  // CARGAR INVENTARIO DESDE EL BACKEND

  cargarInventario(): void {

    this.cargandoInventario = true;

    this.inventarioService.obtenerPaginado(
      this.buscar,
      this.materialFiltro,
      this.colorFiltro,
      this.pagina,
      this.pageSize
    ).subscribe({

      next: (resp: InventarioPaginadoResponse) => {

        this.inventario = resp.items ?? [];

        this.resumen = resp.resumen ?? this.resumen;

        this.filtros = resp.filtros ?? {
          materiales: [],
          colores: []
        };

        this.totalFiltrado = resp.total ?? 0;
        this.totalPaginas = Math.max(resp.totalPaginas ?? 1, 1);
        this.pagina = resp.pagina ?? this.pagina;

        this.cargandoInventario = false;
      },

      error: (error) => {

        this.cargandoInventario = false;

        this.toastr.error(
          error.error?.message || 'No fue posible cargar el inventario.',
          'Inventario'
        );
      }
    });
  }

  // DETALLE

  verDetalleInventario(item: InventarioBodega): void {

    this.dialog.open(DetalleInvenatrio, {
      width: '850px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      disableClose: false,
      data: item
    });
  }

  // RESUMEN: LOS VALORES VIENEN DEL BACKEND

  get totalProductos(): number {
    return this.resumen.totalProductos;
  }

  get pesoTotal(): number {
    return this.resumen.pesoTotal;
  }

  get cantidadTotal(): number {
    return this.resumen.cantidadTotal;
  }

  get totalColores(): number {
    return this.resumen.totalColores;
  }

  get totalMateriales(): number {
    return this.resumen.totalMateriales;
  }

  get totalReferencias(): number {
    return this.resumen.totalReferencias;
  }

  // OPCIONES DE FILTROS DESDE EL BACKEND

  get materiales(): string[] {
    return this.filtros.materiales;
  }

  get colores(): string[] {
    return this.filtros.colores;
  }

  // FILTROS

  aplicarFiltros(): void {

    this.pagina = 1;
    this.cargarInventario();
  }

  limpiarFiltros(): void {

    this.buscar = '';
    this.materialFiltro = '';
    this.colorFiltro = '';
    this.pagina = 1;

    this.cargarInventario();

    this.toastr.info(
      'Se limpiaron los filtros.',
      'Inventario'
    );
  }

  // PAGINACIÓN DESDE EL BACKEND

  get inventarioPaginado(): InventarioBodega[] {
    return this.inventario;
  }

  get registrosInicio(): number {

    return this.totalFiltrado === 0
      ? 0
      : (this.pagina - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {

    return Math.min(
      this.pagina * this.pageSize,
      this.totalFiltrado
    );
  }

  paginaAnterior(): void {

    if (this.pagina > 1) {
      this.pagina--;
      this.cargarInventario();
    }
  }

  paginaSiguiente(): void {

    if (this.pagina < this.totalPaginas) {
      this.pagina++;
      this.cargarInventario();
    }
  }

  // COLOR VISUAL

  obtenerColor(color: string | null | undefined): string {

    const colores: Record<string, string> = {
      'Blanco': '#FFFFFF',
      'Negro': '#222222',
      'Azul': '#357ABD',
      'Verde': '#8BC34A',
      'Rojo': '#E57373',
      'Transparente': '#E5E7EB'
    };

    return colores[color || ''] || '#90A4AE';
  }

  // REGISTRAR PRODUCTO

  ModalRegistrarProducto(): void {

    const dialogRef = this.dialog.open(CrearInventario, {
      width: '900px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      disableClose: true,
      data: null
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (!resultado) return;

      this.inventarioService.registrarItem(resultado).subscribe({

        next: () => {

          this.toastr.success('Producto registrado correctamente.');

          this.pagina = 1;
          this.cargarInventario();
        },

        error: () => {
          this.toastr.error('No fue posible registrar el producto.');
        }
      });
    });
  }

  // EDITAR INVENTARIO

  editarInventario(item: InventarioBodega): void {

    const dialogRef = this.dialog.open(CrearInventario, {
      width: '900px',
      maxWidth: '95vw',
      maxHeight: '90vh',
      disableClose: true,
      data: item
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (!resultado) return;

      this.inventarioService.editar(item.id, resultado).subscribe({

        next: () => {

          this.toastr.success('Producto actualizado correctamente.');

          this.cargarInventario();
        },

        error: () => {
          this.toastr.error('No fue posible actualizar el producto.');
        }
      });
    });
  }

  // ELIMINAR INVENTARIO

  eliminarInventario(id: number): void {

    const confirmar = window.confirm(
      '¿Estás seguro de eliminar este producto del inventario?'
    );

    if (!confirmar) return;

    this.inventarioService.eliminar(id).subscribe({

      next: (resp) => {

        this.toastr.success(
          resp.message || 'Producto eliminado correctamente.',
          'Inventario'
        );

        // Si se elimina el último registro de una página,
        // regresamos a la página anterior.
        if (this.inventario.length === 1 && this.pagina > 1) {
          this.pagina--;
        }

        this.cargarInventario();
      },

      error: (error) => {

        this.toastr.error(
          error.error?.message || 'No fue posible eliminar el producto.',
          'Inventario'
        );
      }
    });
  }

  // RECALCULAR INVENTARIO

  recalcular(): void {

    if (this.cargando) return;

    this.cargando = true;

    this.inventarioService.recalcular().subscribe({

      next: (resp) => {

        this.toastr.success(
          resp.message || 'Inventario recalculado correctamente.',
          'Inventario'
        );

        this.cargando = false;
        this.cargarInventario();
      },

      error: (error) => {

        this.cargando = false;

        this.toastr.error(
          error.error?.message || 'No fue posible recalcular el inventario.',
          'Inventario'
        );
      }
    });
  }
}
