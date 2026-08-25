import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CorteInventarioService } from '../../../../core/services/costos/inventario/cortesinventario/corteinventario.service';
import { InventarioCorte } from '../../../../core/models/costos/inventario/cortesinventario/corteinventario.models';

@Component({
  selector: 'app-corte-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './corte-inventario.html',
  styleUrl: './corte-inventario.scss'
})
export class CorteInventario implements OnInit {

  items: InventarioCorte[] = [];

  constructor(
    private toastr: ToastrService,
    private corteService: CorteInventarioService,
    private dialogRef: MatDialogRef<CorteInventario>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    this.cargarCorte();
  }

  cargarCorte() {
    this.corteService.obtenerCorte().subscribe({
      next: (data) => {
        this.items = data.map(item => ({
          materialId: item.materialId,
          proveedor: item.proveedor,
          material: item.material,
          color: item.color,
          sistema: item.sistema,
          conteo: 0,
          diferencia: -item.sistema
        }));
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('No se pudo cargar el inventario', 'Error');
      }
    });
  }

  actualizar(item: InventarioCorte) {
    item.diferencia = item.conteo - item.sistema;
  }

  imprimirPdf(): void {
    this.corteService.generarPdf().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');

        setTimeout(() => {
          window.URL.revokeObjectURL(url);
        }, 3000);
      },
      error: () => {
        this.toastr.error(
          'No fue posible generar la hoja de corte.',
          'Error'
        );
      }
    });
  }

  guardar() {
    const dto = {
      detalles: this.items.map(item => ({
        materialId: item.materialId,
        color: item.color,
        conteo: item.conteo
      }))
    };

    this.corteService.guardarCorte(dto).subscribe({
      next: () => {
        this.toastr.success('El corte de inventario fue guardado correctamente', 'Corte guardado');
        this.dialogRef.close(true);
      },
      error: (err) => {
        const mensaje = err.error?.mensaje || 'No se pudo guardar el corte';
        this.toastr.error(mensaje, 'Error');
      }
    });
  }

  cerrarModal() {
    this.dialogRef.close();
  }
}