import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CorteInventarioService } from '../../../../core/services/costos/inventario/cortesinventario/corteinventario.service';
import {
  FiltrosCorteInventario,
  InventarioCorte
} from '../../../../core/models/costos/inventario/cortesinventario/corteinventario.models';

@Component({
  selector: 'app-corte-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './corte-inventario.html',
  styleUrl: './corte-inventario.scss'
})
export class CorteInventario implements OnInit {

  items: InventarioCorte[] = [];
  itemsFiltrados: InventarioCorte[] = [];

  filtros: FiltrosCorteInventario = {
    proveedores: [],
    materiales: []
  };

  proveedorSeleccionado = '';

  // Ahora permite escribir el nombre del material
  materialSeleccionado = '';

  constructor(
    private toastr: ToastrService,
    private corteService: CorteInventarioService,
    private dialogRef: MatDialogRef<CorteInventario>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    this.cargarFiltros();
    this.cargarCorte();
  }

  cargarFiltros(): void {
    this.corteService.obtenerFiltros().subscribe({
      next: (data) => {
        this.filtros = data;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(
          'No se pudieron cargar los filtros',
          'Error'
        );
      }
    });
  }

  cargarCorte(): void {
    this.corteService.obtenerCorte().subscribe({
      next: (data) => {
        this.items = data.map(item => ({
          materialId: item.materialId,
          proveedor: item.proveedor,
          material: item.material,
          color: item.color,
          sistema: item.sistema,
          conteo: item.sistema,
          diferencia: 0
        }));

        this.filtrar();
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(
          'No se pudo cargar el inventario',
          'Error'
        );
      }
    });
  }

  get materialesFiltrados() {
    if (!this.proveedorSeleccionado) {
      return this.filtros.materiales;
    }

    return this.filtros.materiales.filter(
      x => x.proveedor === this.proveedorSeleccionado
    );
  }

  filtrar(): void {
    const textoMaterial = this.materialSeleccionado
      .trim()
      .toLocaleLowerCase();

    const proveedor = this.proveedorSeleccionado
      .trim()
      .toLocaleLowerCase();

    this.itemsFiltrados = this.items.filter(item => {

      const coincideProveedor =
        !proveedor ||
        item.proveedor.toLocaleLowerCase() === proveedor;

      const coincideMaterial =
        !textoMaterial ||
        item.material.toLocaleLowerCase().includes(textoMaterial);

      return coincideProveedor && coincideMaterial;
    });
  }

  cambiarProveedor(): void {
    this.filtrar();
  }

  cambiarMaterial(): void {
    this.filtrar();
  }

  actualizar(item: InventarioCorte): void {
    item.diferencia = item.conteo - item.sistema;
  }

  imprimirPdf(): void {
    this.corteService.generarPdf(
      this.materialSeleccionado,
      this.proveedorSeleccionado
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const ventana = window.open(url, '_blank');

        if (!ventana) {
          window.URL.revokeObjectURL(url);
          this.toastr.warning(
            'Permite las ventanas emergentes para abrir el PDF.',
            'Aviso'
          );
          return;
        }

        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 60000);
      },
      error: (err) => {
        console.error(err);
        this.toastr.error(
          'No fue posible generar la hoja de corte.',
          'Error'
        );
      }
    });
  }

  guardar(): void {
    const dto = {
      detalles: this.items.map(item => ({
        materialId: item.materialId,
        color: item.color,
        conteo: item.conteo
      }))
    };

    this.corteService.guardarCorte(dto).subscribe({
      next: () => {
        this.toastr.success(
          'El corte de inventario fue guardado correctamente',
          'Corte guardado'
        );

        this.dialogRef.close(true);
      },
      error: (err) => {
        const mensaje =
          err.error?.mensaje || 'No se pudo guardar el corte';

        this.toastr.error(mensaje, 'Error');
      }
    });
  }

  limpiarFiltros(): void {
    this.proveedorSeleccionado = '';
    this.materialSeleccionado = '';
    this.filtrar();
  }

  cerrarModal(): void {
    this.dialogRef.close();
  }
}
