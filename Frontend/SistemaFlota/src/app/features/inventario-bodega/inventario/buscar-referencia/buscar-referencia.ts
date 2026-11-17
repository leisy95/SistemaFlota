
import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatDialogRef } from '@angular/material/dialog';
import * as XLSX from 'xlsx';

import { InventarioBodega } from '../../../../core/models/inventario-bodega/inventario-bodega.models';
import { InventarioBodegaService } from '../../../../core/services/inventario-bodega/Inventario/inventario-bodega.service';

@Component({
  selector: 'app-buscar-referencia',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './buscar-referencia.html',
  styleUrl: './buscar-referencia.scss',
})
export class BuscarReferencia implements OnInit {

  referenciaControl = new FormControl('');

  referencias: string[] = [];

  inventarioGeneral: InventarioBodega | null = null;

  items: any[] = [];
  itemsFiltrados: any[] = [];

  filtro = '';

  pagina = 1;
  pageSize = 5;

  constructor(
    private inventarioService: InventarioBodegaService,
    private dialogRef: MatDialogRef<BuscarReferencia>
  ) { }

  ngOnInit(): void {
    this.cargarReferencias();
  }

  // CARGAR REFERENCIAS
  cargarReferencias(): void {

    this.inventarioService.obtenerSugerencias().subscribe({
      next: (res) => {
        this.referencias = res;
      },
      error: (error) => {
        console.error('Error al cargar referencias:', error);
        this.referencias = [];
      }
    });
  }

  // SELECCIONAR REFERENCIA
  seleccionarReferencia(): void {

    const referencia = this.referenciaControl.value?.trim();

    if (!referencia) return;

    this.buscarReferencia(referencia);
  }

  // BUSCAR REFERENCIA
  buscarReferencia(referencia: string): void {

    this.inventarioGeneral = null;
    this.items = [];
    this.itemsFiltrados = [];
    this.filtro = '';
    this.pagina = 1;

    this.inventarioService
      .obtenerPorReferencia(referencia)
      .subscribe({

        next: (inventario) => {

          this.inventarioGeneral = inventario;

          this.inventarioService
            .obtenerItems(inventario.id)
            .subscribe({

              next: (respuestaItems) => {

                this.items = respuestaItems;
                this.itemsFiltrados = [...respuestaItems];

                this.pagina = 1;
              },

              error: (error) => {

                console.error('Error al consultar los bultos:', error);

                this.items = [];
                this.itemsFiltrados = [];
              }
            });
        },

        error: (error) => {

          console.error('Error al consultar el inventario:', error);

          this.inventarioGeneral = null;
          this.items = [];
          this.itemsFiltrados = [];
          this.filtro = '';
          this.pagina = 1;
        }
      });
  }

  // FILTRAR RESULTADOS
  applyFilter(event: Event): void {

    const valor = (event.target as HTMLInputElement).value;

    this.filtro = valor;
    const texto = valor.trim().toLowerCase();

    this.itemsFiltrados = this.items.filter(item => {

      const fecha = item.fechaRegistroItem
        ? new Date(item.fechaRegistroItem)
        : null;

      const fechaTexto = fecha && !isNaN(fecha.getTime())
        ? fecha.toLocaleDateString('es-CO', {
          day: '2-digit',
          month: 'long',
          year: 'numeric'
        }).toLowerCase()
        : '';

      return (
        (item.referenciaPeso || '').toLowerCase().includes(texto) ||
        item.pesoActual?.toString().includes(texto) ||
        (item.estado || '').toLowerCase().includes(texto) ||
        fechaTexto.includes(texto)
      );
    });

    this.pagina = 1;
  }

  // PAGINACIÓN
  get itemsPaginados(): any[] {

    const inicio = (this.pagina - 1) * this.pageSize;

    return this.itemsFiltrados.slice(
      inicio,
      inicio + this.pageSize
    );
  }

  get totalPaginas(): number {

    return Math.max(
      1,
      Math.ceil(this.itemsFiltrados.length / this.pageSize)
    );
  }

  get registrosInicio(): number {

    return this.itemsFiltrados.length === 0
      ? 0
      : (this.pagina - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {

    return Math.min(
      this.pagina * this.pageSize,
      this.itemsFiltrados.length
    );
  }

  paginaAnterior(): void {

    if (this.pagina > 1) {
      this.pagina--;
    }
  }

  paginaSiguiente(): void {

    if (this.pagina < this.totalPaginas) {
      this.pagina++;
    }
  }

  cambiarPageSize(valor: number): void {

    this.pageSize = valor;
    this.pagina = 1;
  }

  // EXPORTAR EXCEL
  exportarExcel(): void {

    if (this.itemsFiltrados.length === 0) return;

    const datos = this.itemsFiltrados.map(item => ({
      'Referencia peso': item.referenciaPeso,
      'Peso unitario (kg)': item.pesoActual,
      'Estado': item.estado,
      'Fecha de registro': item.fechaRegistroItem
        ? new Date(item.fechaRegistroItem).toLocaleDateString('es-CO')
        : ''
    }));

    const hoja = XLSX.utils.json_to_sheet(datos);
    const libro = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(
      libro,
      hoja,
      'Items inventario'
    );

    XLSX.writeFile(libro, 'inventario_referencia.xlsx');
  }

  // CERRAR MODAL
  regresar(): void {
    this.dialogRef.close();
  }
}
