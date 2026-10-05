import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InventarioCorteNoFormalizado } from '../../../../core/models/compras-no-formalizadas/inventario/cortes-inventario/corte-inventario-no-formalizado.model';
import { ToastrService } from 'ngx-toastr';
import { CorteInventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/cortesinvenatrio/corte-inventario-no-formalizado.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-corte-inventario-no-formalizado',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule],
  templateUrl: './corte-inventario-no-formalizado.html',
  styleUrl: './corte-inventario-no-formalizado.scss',
})
export class CorteInventarioNoFormalizado implements OnInit {

  items: InventarioCorteNoFormalizado[] = [];

  constructor(
    private toastr: ToastrService,
    private corteService: CorteInventarioNoFormalizadoService,
    private dialogRef: MatDialogRef<CorteInventarioNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) { }

  ngOnInit(): void {
    this.cargarCorte();
  }

  cargarCorte() {
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

  actualizar(item: InventarioCorteNoFormalizado) {
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
          'No fue posible generar la hoja de corte no formalizada.',
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

        this.toastr.error(
          mensaje,
          'Error'
        );
      }
    });
  }

  cerrarModal() {
    this.dialogRef.close();
  }
}
