
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxBarcode6 } from 'ngx-barcode6';

interface Inventario {
  referencia: string;
  referenciaPeso: string;
  ancho: number;
  fuelleLateral: number;
  alto: number;
  fuelleFondo: number;
  fuelleSuperior: number;
  unidad: string;
  calibre: number;
  pesoActual: number;
  tipoMaterial: string;
  densidad: number;
  color: string;
  troquel: string;
  paqueteNumero: number;
  paqueteCantidad: number;
  referenciaImpresa: string;
}

@Component({
  selector: 'app-registrar-entrada',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgxBarcode6
  ],
  templateUrl: './registrar-entrada.html',
  styleUrl: './registrar-entrada.scss',
})
export class RegistrarEntrada {

  referencia = '';
  pesoIngresado: number | null = null;

  inventario: Inventario | null = null;

  pesoMostrado: number | null = null;
  pesoActual: number | null = null;

  fechaImpresion = new Date();

  barcodeValue = '';

  inventarios: Inventario[] = [
    {
      referencia: 'MAT-001',
      referenciaPeso: '4R0TR0012.0*018.0*1.30',
      ancho: 900,
      fuelleLateral: 100,
      alto: 900,
      fuelleFondo: 0,
      fuelleSuperior: 0,
      unidad: 'PG',
      calibre: 4,
      pesoActual: 25,
      tipoMaterial: 'PEBD',
      densidad: 0.92,
      color: 'Natural',
      troquel: 'Normal',
      paqueteNumero: 1,
      paqueteCantidad: 100,
      referenciaImpresa: ''
    },
    {
      referencia: 'MAT-002',
      referenciaPeso: '2R0BL9009.0*000.0*5.00',
      ancho: 1000,
      fuelleLateral: 120,
      alto: 800,
      fuelleFondo: 0,
      fuelleSuperior: 0,
      unidad: 'PG',
      calibre: 5,
      pesoActual: 30,
      tipoMaterial: 'PEAD',
      densidad: 0.95,
      color: 'Blanco',
      troquel: 'Especial',
      paqueteNumero: 2,
      paqueteCantidad: 150,
      referenciaImpresa: 'MAT-002'
    },
    {
      referencia: 'MAT-003',
      referenciaPeso: '3R0BL8008.0*000.0*3.00',
      ancho: 800,
      fuelleLateral: 80,
      alto: 800,
      fuelleFondo: 0,
      fuelleSuperior: 0,
      unidad: 'PG',
      calibre: 3,
      pesoActual: 40,
      tipoMaterial: 'PEBD',
      densidad: 0.92,
      color: 'Negro',
      troquel: 'Normal',
      paqueteNumero: 3,
      paqueteCantidad: 200,
      referenciaImpresa: 'MAT-003'
    }
  ];

  get totalReferencias(): number {
    return this.inventarios.length;
  }

  get pesoTotal(): number {
    return this.inventarios.reduce(
      (total, item) => total + item.pesoActual,
      0
    );
  }

  // Código que debe leer el escáner
  getBarcodeValue(): string {
    return this.barcodeValue;
  }

  buscar(): void {
    const codigo = this.referencia.trim().toLowerCase();

    if (!codigo) {
      alert('Ingrese una referencia para consultar.');
      return;
    }

    const resultado = this.inventarios.find(item =>
      item.referenciaPeso.toLowerCase() === codigo ||
      item.referencia.toLowerCase() === codigo
    );

    if (!resultado) {
      this.inventario = null;
      this.barcodeValue = '';
      alert('No se encontró la referencia.');
      return;
    }

    this.inventario = resultado;
    this.barcodeValue = resultado.referenciaPeso;

    this.pesoIngresado = null;
    this.pesoMostrado = resultado.pesoActual;
    this.pesoActual = resultado.pesoActual;

    this.fechaImpresion = new Date();
  }

  confirmarIngresoPeso(): void {
    if (!this.inventario) {
      alert('Primero debe consultar una referencia.');
      return;
    }

    const peso = Number(this.pesoIngresado);

    if (!peso || peso <= 0) {
      alert('Ingrese un peso válido.');
      return;
    }

    this.inventario.pesoActual += peso;

    this.pesoMostrado = peso;
    this.pesoActual = this.inventario.pesoActual;

    this.barcodeValue = this.inventario.referenciaPeso;

    this.fechaImpresion = new Date();

    this.pesoIngresado = null;

    alert('Peso registrado correctamente.');

    this.imprimirEtiqueta();
  }

  limpiar(): void {
    this.referencia = '';
    this.pesoIngresado = null;
    this.pesoMostrado = null;
    this.pesoActual = null;
    this.inventario = null;
    this.barcodeValue = '';
  }

  imprimirEtiqueta(): void {
    if (!this.inventario) {
      alert('No hay una referencia seleccionada.');
      return;
    }

    this.fechaImpresion = new Date();

    setTimeout(() => {
      window.print();
    }, 300);
  }
}
