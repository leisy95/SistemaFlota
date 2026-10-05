import { Component, Inject } from '@angular/core';
import { OrdenTrasladoNoFormalizada } from '../../../../core/models/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizado.models';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { OrdenTrasladoNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizada.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface MaterialVerificacionNoFormalizada {
  id: number;
  materialNoFormalizadoId: number | null;
  material: string;
  proveedor: string;
  tipo: string;
  densidad: string;
  color: string;
  cantidadKg: number;
  bultos: number;
  cantidadVerificadaKg: number | null;
  bultosVerificados: number | null;
  estadoVerificacion: string;
  cantidadEncontrada: number;
  bultosEncontrados: number;
}

@Component({
  selector: 'app-verificar-orden-traslado-no-formalizado',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verificar-orden-traslado-no-formalizado.html',
  styleUrl: './verificar-orden-traslado-no-formalizado.scss',
})
export class VerificarOrdenTrasladoNoFormalizado {
  orden: OrdenTrasladoNoFormalizada;
  observaciones = '';
  procesando = false;
  materiales: MaterialVerificacionNoFormalizada[] = [];

  constructor(
    private dialogRef: MatDialogRef<VerificarOrdenTrasladoNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public data: OrdenTrasladoNoFormalizada,
    private ordenTrasladoService: OrdenTrasladoNoFormalizadaService,
    private toastr: ToastrService
  ) {
    this.orden = data;
    this.materiales = this.orden.materiales.map(material => ({
      ...material,
      cantidadEncontrada: material.cantidadVerificadaKg ?? material.cantidadKg,
      bultosEncontrados: material.bultosVerificados ?? material.bultos
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
    return this.materiales.filter(material => this.estadoMaterial(material) === 'Completo').length;
  }

  get totalKgVerificados(): number {
    return this.materiales.reduce(
      (total, material) => total + Number(material.cantidadEncontrada || 0), 0
    );
  }

  get totalBultosVerificados(): number {
    return this.materiales.reduce(
      (total, material) => total + Number(material.bultosEncontrados || 0), 0
    );
  }

  estadoMaterial(material: MaterialVerificacionNoFormalizada): string {
    if (material.cantidadEncontrada === 0 || material.bultosEncontrados === 0) {
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
    if (this.procesando) return;
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
      this.toastr.warning('Debe verificar todos los materiales.', 'Verificación');
      return;
    }

    const dto = {
      ordenTrasladoNoFormalizadaId: this.orden.id,
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
          'Los materiales no formalizados fueron verificados correctamente.',
          'Orden de traslado no formalizada'
        );
        this.dialogRef.close(true);
      },
      error: error => {
        this.procesando = false;
        this.toastr.error(
          error?.error?.mensaje || 'No fue posible finalizar la verificación.',
          'Error'
        );
      }
    });
  }
}