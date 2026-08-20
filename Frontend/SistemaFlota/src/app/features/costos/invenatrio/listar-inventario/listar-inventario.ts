import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Inventario_Costos } from '../../../../core/models/costos/inventario/inventario-costos.models';
import { InventarioService } from '../../../../core/services/costos/inventario/inventario.service';
import { ProveedorFiltro } from '../../../../core/models/costos/inventario/proveedorfiltro.models';
import { EstadoListadoService } from '../../../../core/services/serviciogeneralcarga/EstadoListadoService';
import { MatDialog } from '@angular/material/dialog';
import { CorteInventario } from '../corte-inventario/corte-inventario';
import { AjusteInventario } from '../ajuste-inventario/ajuste-inventario';
import { HistorialInventario } from '../historial-inventario/historial-inventario';
import { HistorialCorteInventario } from '../historial-corte-inventario/historial-corte-inventario';

@Component({
  selector: 'app-listar-inventario',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './listar-inventario.html',
  styleUrl: './listar-inventario.scss',
})
export class ListarInventario {

  private timeoutBusqueda: any;

  totalItems = 0;
  totalKg = 0;
  totalBultos = 0;
  valorInventario = 0;

  buscar = '';
  proveedorId: number | null = null;
  tipoMaterial: string | null = null;

  proveedores: ProveedorFiltro[] = [];
  categorias: string[] = [];

  materiales: Inventario_Costos[] = [];
  materialSeleccionadoId = 0;

  cpp = {
    stockActual: 0,
    costoActual: 0,
    compraNueva: 0,
    precioNuevo: 0
  };

  mostrarResultado = false;

  resultadoCpp = {
    nuevoCosto: 0,
    valorCompra: 0,
    nuevoInventario: 0,
    nuevoStock: 0
  };

  page = 1;
  pageSize = 20;
  totalRegistros = 0;

  pageSizes = [10, 20, 50, 100];

  get totalPaginas(): number {
    return Math.ceil(this.totalRegistros / this.pageSize);
  }

  get hastaRegistro(): number {
    return Math.min(this.page * this.pageSize, this.totalRegistros);
  }

  constructor(
    private toastr: ToastrService,
    private inventarioService: InventarioService,
    private estado: EstadoListadoService,
    private dialog: MatDialog,
  ) { }

  ngOnInit(): void {

    this.cargarCategorias();
    this.cargarProveedores();

    const estado = this.estado.obtener('inventario');

    if (estado) {
      this.buscar = estado.buscar;
      this.proveedorId = estado.proveedorId;
      this.tipoMaterial = estado.tipoMaterial;
      this.page = estado.page;
      this.pageSize = estado.pageSize;
    }

    this.cargarInventario();
  }

  private guardarEstado(): void {

    this.estado.guardar('inventario', {
      buscar: this.buscar,
      proveedorId: this.proveedorId,
      tipoMaterial: this.tipoMaterial,
      page: this.page,
      pageSize: this.pageSize
    });

  }

  cargarInventario(): void {
    this.inventarioService.obtener(
      this.buscar,
      this.proveedorId,
      this.tipoMaterial === 'Todos' ? null : this.tipoMaterial,
      null,
      this.page,
      this.pageSize
    ).subscribe({
      next: (resp) => {
        this.materiales = resp.items;
        this.totalRegistros = resp.total;
        this.totalItems = resp.total;
        this.totalKg = resp.items.reduce((s: number, x: Inventario_Costos) => s + x.stockActual, 0);
        this.valorInventario = resp.items.reduce((s: number, x: Inventario_Costos) => s + x.valorInventario, 0);
      },
      error: () => {
        this.toastr.error('No fue posible cargar el inventario');
      }
    });
  }

  buscarInventario(): void {

    clearTimeout(this.timeoutBusqueda);

    this.timeoutBusqueda = setTimeout(() => {

      this.page = 1;
      this.guardarEstado();
      this.cargarInventario();

    }, 400);

  }

  filtrar(): void {

    this.page = 1;
    this.guardarEstado();
    this.cargarInventario();

  }

  limpiarFiltros(): void {

    this.buscar = '';
    this.proveedorId = null;
    this.tipoMaterial = null;
    this.page = 1;

    this.guardarEstado();
    this.cargarInventario();

  }

  calcularCPP(): void {
    if (this.cpp.compraNueva <= 0 || this.cpp.precioNuevo <= 0) {
      this.mostrarResultado = false;
      return;
    }

    const valorActual = this.cpp.stockActual * this.cpp.costoActual;
    const valorCompra = this.cpp.compraNueva * this.cpp.precioNuevo;
    const nuevoStock = this.cpp.stockActual + this.cpp.compraNueva;
    const nuevoInventario = valorActual + valorCompra;
    const nuevoCosto = nuevoInventario / nuevoStock;

    this.resultadoCpp = {
      nuevoCosto,
      valorCompra,
      nuevoInventario,
      nuevoStock
    };

    this.mostrarResultado = true;
  }

  cargarProveedores(): void {
    this.inventarioService.obtenerProveedores().subscribe({
      next: resp => this.proveedores = resp
    });
  }

  cargarCategorias(): void {
    this.inventarioService.obtenerCategorias().subscribe({
      next: resp => this.categorias = resp
    });
  }

  seleccionarMaterial(): void {
    const material = this.materiales.find(x => x.id === this.materialSeleccionadoId);
    if (!material) return;

    this.cpp.stockActual = material.stockActual;
    this.cpp.costoActual = material.costoPromedio;
    this.cpp.compraNueva = 0;
    this.cpp.precioNuevo = 0;
    this.mostrarResultado = false;
  }

  exportarExcel(): void {

    this.inventarioService.exportarExcel(
      this.buscar,
      this.proveedorId,
      this.tipoMaterial,
      null
    ).subscribe({

      next: (archivo: Blob) => {

        const blob = new Blob([archivo], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });

        const url = window.URL.createObjectURL(blob);

        const a = document.createElement('a');
        a.href = url;
        a.download = `Inventario_${new Date().toISOString().slice(0, 10)}.xlsx`;
        a.click();

        window.URL.revokeObjectURL(url);

        this.toastr.success(
          'Archivo exportado correctamente.',
          'Inventario'
        );

      },

      error: () => {
        this.toastr.error(
          'No fue posible exportar el inventario.'
        );
      }
    });
  }

  corteMes(): void {

    const dialogRef = this.dialog.open(CorteInventario, {
      width: '1200px',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '95vh',
      disableClose: true,
      data: {
        titulo: 'Corte de Mes'
      }
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (resultado) {
        this.toastr.success(
          'Corte de mes realizado.',
          'Inventario'
        );
      }

    });

  }

  imprimir(material: any): void {
    this.toastr.success(
      `Imprimiendo ${material.tipo}.`,
      'Inventario'
    );
  }

  editar(material: Inventario_Costos): void {

    this.dialog.open(AjusteInventario, {
      width: '650px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      panelClass: 'ajuste-dialog',
      data: material.id
    });

  }

  historial(material: Inventario_Costos): void {

    this.dialog.open(HistorialInventario, {
      width: '1100px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      data: material.id
    });

  }

  historialCortes(): void {
    this.dialog.open(HistorialCorteInventario, {
      width: '1000px',
      maxWidth: '95vw'
    });
  }

  cambiarPagina(pagina: number): void {

    if (pagina < 1 || pagina > this.totalPaginas) return;

    this.page = pagina;
    this.guardarEstado();
    this.cargarInventario();

  }

  cambiarPageSize(): void {

    this.page = 1;
    this.guardarEstado();
    this.cargarInventario();

  }
}

