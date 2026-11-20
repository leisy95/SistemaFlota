
import { CommonModule } from '@angular/common';
import { Component, ElementRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxBarcode6 } from 'ngx-barcode6';

import {
  IngresarPesoDto,
  IngresarPesoResponseDto,
  InventarioBodega
} from '../../../../core/models/inventario-bodega/inventario-bodega.models';

import { InventarioBodegaService } from '../../../../core/services/inventario-bodega/Inventario/inventario-bodega.service';
import { EntradaInventarioService } from '../../../../core/services/inventario-bodega/entrada-inventario/entrada-inventario.service';
import { DialogService } from '../../../../core/services/dialogs/dialog.service';

@Component({
  selector: 'app-registrar-entrada',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgxBarcode6,
  ],
  templateUrl: './registrar-entrada.html',
  styleUrl: './registrar-entrada.scss',
})
export class RegistrarEntrada {

  @ViewChild('pesoInput') pesoInput!: ElementRef<HTMLInputElement>;

  referencia = '';
  pesoIngresado: number | null = null;

  inventario: InventarioBodega | null = null;

  pesoMostrado: number | null = null;
  pesoActual: number | null = null;
  pesoTotal: number = 0;

  fechaImpresion = new Date();
  barcodeValue = '';

  cargando = false;
  registrando = false;

  // SUGERENCIAS DE REFERENCIAS
  sugerencias: string[] = [];
  sugerenciasVisibles = false;
  cargandoSugerencias = false;

  constructor(
    private inventarioService: InventarioBodegaService,
    private entradaInventarioService: EntradaInventarioService,
    private dialogService: DialogService
  ) { }

  // MOSTRAR SUGERENCIAS AL ENFOCAR EL INPUT
  mostrarSugerencias(): void {
    this.sugerenciasVisibles = true;
    this.cargarSugerencias();
  }

  // CONSULTAR SUGERENCIAS EN EL BACKEND
  cargarSugerencias(): void {
    this.cargandoSugerencias = true;

    this.inventarioService.obtenerSugerencias(this.referencia).subscribe({
      next: (resultado) => {
        this.sugerencias = resultado;
        this.cargandoSugerencias = false;
      },
      error: () => {
        this.sugerencias = [];
        this.cargandoSugerencias = false;
      }
    });
  }

  // FILTRAR SUGERENCIAS MIENTRAS ESCRIBE
  filtrarSugerencias(): void {
    this.sugerenciasVisibles = true;
    this.cargarSugerencias();
  }

  // SELECCIONAR REFERENCIA DE LAS SUGERENCIAS
  seleccionarReferencia(referencia: string): void {
    this.referencia = referencia;
    this.sugerenciasVisibles = false;
    this.sugerencias = [];

    this.buscar();
  }

  // CERRAR SUGERENCIAS
  cerrarSugerencias(): void {
    this.sugerenciasVisibles = false;
  }

  // CONSULTAR REFERENCIA COMPLETA
  buscar(): void {
    const codigo = this.referencia.trim();

    if (!codigo) {
      this.dialogService.alerta(
        'Referencia requerida',
        'Ingrese una referencia para consultar.',
        'warning'
      );
      return;
    }

    this.sugerenciasVisibles = false;
    this.sugerencias = [];

    this.cargando = true;
    this.inventario = null;
    this.barcodeValue = '';
    this.pesoMostrado = null;
    this.pesoActual = null;

    // ESTA CONSULTA SE CONSERVA:
    // Obtiene todos los datos del inventario seleccionado.
    this.inventarioService.obtenerPorReferencia(codigo).subscribe({
      next: (resultado) => {
        this.inventario = resultado;

        this.pesoActual = resultado.peso;
        this.pesoMostrado = resultado.peso;
        this.pesoTotal = resultado.peso;

        this.barcodeValue = resultado.referencia;
        this.fechaImpresion = new Date();

        this.pesoIngresado = null;
        this.cargando = false;

        setTimeout(() => {
          this.pesoInput?.nativeElement.focus();
        }, 100);
      },
      error: (error) => {
        this.cargando = false;
        this.inventario = null;
        this.barcodeValue = '';

        const mensaje =
          error.error?.message || 'No se encontró la referencia.';

        this.dialogService.alerta(
          'Referencia no encontrada',
          mensaje,
          'error'
        );
      }
    });
  }

  // REGISTRAR ENTRADA DE PESO
  confirmarIngresoPeso(): void {
    if (!this.inventario) {
      this.dialogService.alerta(
        'Referencia requerida',
        'Primero debe consultar una referencia.',
        'warning'
      );
      return;
    }

    const peso = Number(this.pesoIngresado);

    if (!Number.isFinite(peso) || peso <= 0) {
      this.dialogService.alerta(
        'Peso inválido',
        'Ingrese un peso válido.',
        'warning'
      );
      return;
    }

    const dialogRef = this.dialogService.alerta(
      'Confirmar Ingreso',
      `¿Seguro deseas ingresar el peso ${peso} a la referencia ${this.inventario.referencia}?`,
      'question',
      true,
      'Aceptar',
      'Cancelar'
    );

    dialogRef.afterClosed().subscribe(confirmado => {
      if (!confirmado) {
        return;
      }

      const request: IngresarPesoDto = {
        referencia: this.inventario!.referencia,
        peso: peso
      };

      this.registrando = true;

      this.entradaInventarioService.ingresarPeso(request).subscribe({
        next: (respuesta: IngresarPesoResponseDto) => {
          this.inventario!.peso = respuesta.peso;
          this.pesoActual = respuesta.peso;
          this.pesoMostrado = peso;
          this.barcodeValue = respuesta.referenciaPeso;
          this.fechaImpresion = new Date();
          this.pesoIngresado = null;
          this.registrando = false;

          const successDialog = this.dialogService.alerta(
            '¡Éxito!',
            'Peso registrado correctamente.',
            'success'
          );

          successDialog.afterClosed().subscribe(() => {
            this.imprimirEtiqueta();
          });
        },
        error: (error) => {
          this.registrando = false;

          const mensaje =
            error.error?.message || 'Error al registrar el peso.';

          this.dialogService.alerta(
            'Error',
            mensaje,
            'error'
          );
        }
      });
    });
  }

  // LIMPIAR FORMULARIO
  limpiar(): void {
    this.referencia = '';
    this.pesoIngresado = null;
    this.pesoMostrado = null;
    this.pesoActual = null;
    this.inventario = null;
    this.barcodeValue = '';
    this.fechaImpresion = new Date();

    this.sugerencias = [];
    this.sugerenciasVisibles = false;
  }

  // IMPRIMIR ETIQUETA
  imprimirEtiqueta(): void {
    if (!this.inventario || !this.barcodeValue) {
      this.dialogService.alerta(
        'Atención',
        'No hay una referencia seleccionada para imprimir.',
        'warning'
      );
      return;
    }

    this.fechaImpresion = new Date();

    setTimeout(() => {
      window.print();
    }, 300);
  }
}
