import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import {
  OrdenTraslado,
  OrdenTrasladoDetalle
} from '../../../../core/models/costos/OrdenesTraslado/orden-traslado.model';

import { OrdenTrasladoService } from '../../../../core/services/costos/ordenestraslado/ordentraslado.service';

interface MaterialVerificacion extends OrdenTrasladoDetalle {
  cantidadEncontrada: number;
  bultosEncontrados: number;
}

@Component({
  selector: 'app-verificar-orden-traslado',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verificar-orden-traslado.html',
  styleUrl: './verificar-orden-traslado.scss',
})
export class VerificarOrdenTraslado {

  orden: OrdenTraslado;
  observaciones = '';
  procesando = false;

  materiales: MaterialVerificacion[] = [];

  constructor(
    private dialogRef: MatDialogRef<VerificarOrdenTraslado>,
    @Inject(MAT_DIALOG_DATA) public data: OrdenTraslado,
    private ordenTrasladoService: OrdenTrasladoService,
    private toastr: ToastrService
  ) {
    this.orden = data;

    this.materiales = this.orden.materiales.map(material => ({
      ...material,
      cantidadEncontrada: material.cantidadVerificadaKg ?? 0,
      bultosEncontrados: material.bultosVerificados ?? 0
    }));
  }

  get todosCompletos(): boolean {
    return this.materiales.every(material =>
      material.cantidadEncontrada === material.cantidadKg &&
      material.bultosEncontrados === material.bultos
    );
  }

  get hayCantidadesInvalidas(): boolean {
    return this.materiales.some(material =>
      material.cantidadEncontrada < 0 ||
      material.bultosEncontrados < 0 ||
      material.cantidadEncontrada > material.cantidadKg ||
      material.bultosEncontrados > material.bultos
    );
  }

  get materialesCompletos(): number {
    return this.materiales.filter(
      material => this.estadoMaterial(material) === 'Completo'
    ).length;
  }

  get totalKgVerificados(): number {
    return this.materiales.reduce(
      (total, material) =>
        total + Number(material.cantidadEncontrada || 0),
      0
    );
  }

  get totalBultosVerificados(): number {
    return this.materiales.reduce(
      (total, material) =>
        total + Number(material.bultosEncontrados || 0),
      0
    );
  }

  estadoMaterial(material: MaterialVerificacion): string {

    if (
      material.cantidadEncontrada === 0 ||
      material.bultosEncontrados === 0
    ) {
      return 'NoDisponible';
    }

    if (
      material.cantidadEncontrada < material.cantidadKg ||
      material.bultosEncontrados < material.bultos
    ) {
      return 'Parcial';
    }

    return 'Completo';
  }

  cerrar(): void {
    if (this.procesando) {
      return;
    }

    this.dialogRef.close();
  }

  finalizarVerificacion(): void {

    if (this.hayCantidadesInvalidas) {
      this.toastr.warning(
        'Las cantidades encontradas no pueden superar las cantidades solicitadas.',
        'Verificación'
      );
      return;
    }

    if (this.materiales.length !== this.orden.materiales.length) {
      this.toastr.warning(
        'Debe verificar todos los materiales.',
        'Verificación'
      );
      return;
    }

    const dto = {
      ordenTrasladoId: this.orden.id,
      observaciones: this.observaciones || null,
      materiales: this.materiales.map(material => ({
        detalleId: material.id,
        cantidadVerificadaKg: Number(material.cantidadEncontrada),
        bultosVerificados: Number(material.bultosEncontrados)
      }))
    };

    this.procesando = true;

    this.ordenTrasladoService.verificar(dto).subscribe({
      next: () => {

        this.procesando = false;

        this.toastr.success(
          'Los materiales fueron verificados correctamente.',
          'Orden de traslado'
        );

        this.dialogRef.close(true);
      },

      error: error => {

        this.procesando = false;

        this.toastr.error(
          error?.error?.mensaje ||
          'No fue posible finalizar la verificación.',
          'Error'
        );
      }
    });
  }
}