import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { InventarioCorteNoFormalizado } from '../../../../core/models/compras-no-formalizadas/inventario/cortes-inventario/corte-inventario-no-formalizado.model';

import { CorteInventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/cortesinvenatrio/corte-inventario-no-formalizado.service';

@Component({
  selector: 'app-corte-inventario-no-formalizado',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './corte-inventario-no-formalizado.html',
  styleUrl: './corte-inventario-no-formalizado.scss',
})
export class CorteInventarioNoFormalizado implements OnInit {

  items: InventarioCorteNoFormalizado[] = [];
  itemsFiltrados: InventarioCorteNoFormalizado[] = [];

  materialSeleccionado = '';
  proveedorSeleccionado = '';

  proveedores: string[] = [];
  materiales: string[] = [];

  constructor(
    private toastr: ToastrService,
    private corteService: CorteInventarioNoFormalizadoService,
    private dialogRef: MatDialogRef<CorteInventarioNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    this.cargarCorte();
  }

  cargarCorte(): void {
    this.corteService.obtenerCorte().subscribe({
      next: (data) => {
        this.items = data.map(item => ({
          inventarioId: item.inventarioId,
          materialId: item.materialId,
          proveedor: item.proveedor,
          material: item.material,
          color: item.color,
          sistema: item.sistema,
          conteo: 0,
          diferencia: -item.sistema
        }));

        this.cargarFiltros();
        this.filtrar();
      },
      error: (err) => {
        console.error(err);

        this.toastr.error(
          'No se pudo cargar el inventario no formalizado',
          'Error'
        );
      }
    });
  }

  cargarFiltros(): void {
    this.proveedores = [
      ...new Set(
        this.items
          .map(item => item.proveedor)
          .filter(proveedor =>
            typeof proveedor === 'string' &&
            proveedor.trim() !== ''
          )
      )
    ].sort((a, b) => a.localeCompare(b));

    this.actualizarMateriales();
  }

  get materialesFiltrados(): string[] {
    if (!this.proveedorSeleccionado) {
      return this.materiales;
    }

    const materialesProveedor = this.items
      .filter(item => item.proveedor === this.proveedorSeleccionado)
      .map(item => item.material);

    return [...new Set(materialesProveedor)]
      .sort((a, b) => a.localeCompare(b));
  }

  actualizarMateriales(): void {
    this.materiales = [
      ...new Set(
        this.items
          .map(item => item.material)
          .filter(material =>
            typeof material === 'string' &&
            material.trim() !== ''
          )
      )
    ].sort((a, b) => a.localeCompare(b));
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
        (item.proveedor ?? '').toLocaleLowerCase() === proveedor;

      const coincideMaterial =
        !textoMaterial ||
        (item.material ?? '')
          .toLocaleLowerCase()
          .includes(textoMaterial);

      return coincideProveedor && coincideMaterial;
    });
  }

  cambiarProveedor(): void {
    // Si el material escrito no pertenece al proveedor,
    // se conserva el texto y se aplican ambos filtros.
    this.filtrar();
  }

  cambiarMaterial(): void {
    this.filtrar();
  }

  actualizar(item: InventarioCorteNoFormalizado): void {
    item.diferencia = item.conteo - item.sistema;
  }

  imprimirPdf(): void {
    this.corteService.generarPdf(
      this.materialSeleccionado,
      this.proveedorSeleccionado
    ).subscribe({
      next: (pdf: Blob) => {
        const url = window.URL.createObjectURL(pdf);
        const ventana = window.open(url, '_blank');

        if (!ventana) {
          const enlace = document.createElement('a');

          enlace.href = url;
          enlace.download = 'CorteInventarioNoFormalizado.pdf';
          enlace.click();

          this.toastr.info(
            'El PDF se descargó porque el navegador bloqueó la ventana emergente.',
            'PDF generado'
          );
        }

        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 60000);
      },
      error: (err) => {
        console.error('Error al generar el PDF:', err);

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
          'El corte de inventario no formalizado fue guardado correctamente',
          'Corte guardado'
        );

        this.dialogRef.close(true);
      },
      error: (err) => {
        const mensaje =
          err.error?.mensaje ||
          'No se pudo guardar el corte de inventario no formalizado';

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
