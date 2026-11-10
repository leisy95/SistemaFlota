import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

interface Inventario {
  id: number;
  referencia: string;
  tipoBolsa: string;
  tipoMaterial: string;
  densidad: string;
  color: string;
  fuelleLateral: string;
  fuelleFondo: string;
  fuelleSuperior: string;
  segundoColor: string;
  impresoNo: string;
  ancho: number;
  alto: number;
  calibre: number;
  peso: number;
  cantidad: number;
  troquel: string;
  referenciaImpresa: string;
  paqueteNumero: number;
  paqueteCantidad: number;
}

@Component({
  selector: 'app-listar-inventario',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './listar-inventario.html',
  styleUrl: './listar-inventario.scss',
})
export class ListarInventario implements OnInit {

  buscar = '';
  materialFiltro = '';
  colorFiltro = '';

  pagina = 1;
  pageSize = 5;

  cargando = false;

  inventario: Inventario[] = [
    {
      id: 1,
      referencia: 'REF-001',
      tipoBolsa: 'Bolsa camiseta',
      tipoMaterial: 'Polietileno',
      densidad: 'Alta',
      color: 'Blanco',
      fuelleLateral: 'Sí',
      fuelleFondo: 'No',
      fuelleSuperior: 'No',
      segundoColor: 'N/A',
      impresoNo: 'Impreso',
      ancho: 30,
      alto: 40,
      calibre: 1.5,
      peso: 25,
      cantidad: 800,
      troquel: 'Estándar',
      referenciaImpresa: 'REF-001',
      paqueteNumero: 1,
      paqueteCantidad: 20
    },
    {
      id: 2,
      referencia: 'REF-002',
      tipoBolsa: 'Bolsa plana',
      tipoMaterial: 'Polipropileno',
      densidad: 'Baja',
      color: 'Transparente',
      fuelleLateral: 'No',
      fuelleFondo: 'Sí',
      fuelleSuperior: 'No',
      segundoColor: 'N/A',
      impresoNo: 'Sin impresión',
      ancho: 25,
      alto: 35,
      calibre: 1.2,
      peso: 18,
      cantidad: 500,
      troquel: 'Estándar',
      referenciaImpresa: 'REF-002',
      paqueteNumero: 2,
      paqueteCantidad: 15
    },
    {
      id: 3,
      referencia: 'REF-003',
      tipoBolsa: 'Bolsa industrial',
      tipoMaterial: 'Polietileno',
      densidad: 'Alta',
      color: 'Negro',
      fuelleLateral: 'Sí',
      fuelleFondo: 'Sí',
      fuelleSuperior: 'No',
      segundoColor: 'Azul',
      impresoNo: 'Impreso',
      ancho: 50,
      alto: 70,
      calibre: 2,
      peso: 40,
      cantidad: 1200,
      troquel: 'Industrial',
      referenciaImpresa: 'REF-003',
      paqueteNumero: 3,
      paqueteCantidad: 30
    },
    {
      id: 4,
      referencia: 'REF-004',
      tipoBolsa: 'Bolsa sello fondo',
      tipoMaterial: 'Polipropileno',
      densidad: 'Media',
      color: 'Azul',
      fuelleLateral: 'No',
      fuelleFondo: 'Sí',
      fuelleSuperior: 'Sí',
      segundoColor: 'N/A',
      impresoNo: 'Sin impresión',
      ancho: 20,
      alto: 30,
      calibre: 1,
      peso: 15,
      cantidad: 350,
      troquel: 'Estándar',
      referenciaImpresa: 'REF-004',
      paqueteNumero: 4,
      paqueteCantidad: 10
    },
    {
      id: 5,
      referencia: 'REF-005',
      tipoBolsa: 'Bolsa camiseta',
      tipoMaterial: 'Polietileno',
      densidad: 'Media',
      color: 'Verde',
      fuelleLateral: 'Sí',
      fuelleFondo: 'No',
      fuelleSuperior: 'No',
      segundoColor: 'N/A',
      impresoNo: 'Impreso',
      ancho: 35,
      alto: 45,
      calibre: 1.8,
      peso: 30,
      cantidad: 950,
      troquel: 'Especial',
      referenciaImpresa: 'REF-005',
      paqueteNumero: 5,
      paqueteCantidad: 25
    },
    {
      id: 6,
      referencia: 'REF-006',
      tipoBolsa: 'Bolsa plana',
      tipoMaterial: 'Polietileno',
      densidad: 'Baja',
      color: 'Blanco',
      fuelleLateral: 'No',
      fuelleFondo: 'No',
      fuelleSuperior: 'No',
      segundoColor: 'N/A',
      impresoNo: 'Sin impresión',
      ancho: 15,
      alto: 25,
      calibre: 1,
      peso: 12,
      cantidad: 400,
      troquel: 'Estándar',
      referenciaImpresa: 'REF-006',
      paqueteNumero: 6,
      paqueteCantidad: 12
    }
  ];

  inventarioFiltrado: Inventario[] = [];

  constructor(
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.aplicarFiltros();
  }

  // INDICADORES
  get totalProductos(): number {
    return this.inventario.length;
  }

  get pesoTotal(): number {
    return this.inventario.reduce((total, item) => total + item.peso, 0);
  }

  get cantidadTotal(): number {
    return this.inventario.reduce((total, item) => total + item.cantidad, 0);
  }

  get totalColores(): number {
    return new Set(this.inventario.map(x => x.color)).size;
  }

  get totalMateriales(): number {
    return new Set(this.inventario.map(x => x.tipoMaterial)).size;
  }

  get totalReferencias(): number {
    return new Set(this.inventario.map(x => x.referencia)).size;
  }

  // OPCIONES DE FILTROS
  get materiales(): string[] {
    return [...new Set(this.inventario.map(x => x.tipoMaterial))];
  }

  get colores(): string[] {
    return [...new Set(this.inventario.map(x => x.color))];
  }

  // FILTROS
  aplicarFiltros(): void {
    const texto = this.buscar.toLowerCase().trim();

    this.inventarioFiltrado = this.inventario.filter(item => {
      const coincideTexto =
        item.referencia.toLowerCase().includes(texto) ||
        item.tipoBolsa.toLowerCase().includes(texto) ||
        item.tipoMaterial.toLowerCase().includes(texto) ||
        item.color.toLowerCase().includes(texto) ||
        item.referenciaImpresa.toLowerCase().includes(texto);

      const coincideMaterial =
        !this.materialFiltro ||
        item.tipoMaterial === this.materialFiltro;

      const coincideColor =
        !this.colorFiltro ||
        item.color === this.colorFiltro;

      return coincideTexto && coincideMaterial && coincideColor;
    });

    this.pagina = 1;
  }

  limpiarFiltros(): void {
    this.buscar = '';
    this.materialFiltro = '';
    this.colorFiltro = '';

    this.aplicarFiltros();

    this.toastr.info(
      'Se limpiaron los filtros.',
      'Inventario'
    );
  }

  // PAGINACIÓN
  get totalFiltrado(): number {
    return this.inventarioFiltrado.length;
  }

  get totalPaginas(): number {
    return Math.max(1, Math.ceil(this.totalFiltrado / this.pageSize));
  }

  get inventarioPaginado(): Inventario[] {
    const inicio = (this.pagina - 1) * this.pageSize;

    return this.inventarioFiltrado.slice(
      inicio,
      inicio + this.pageSize
    );
  }

  get registrosInicio(): number {
    return this.totalFiltrado === 0
      ? 0
      : (this.pagina - 1) * this.pageSize + 1;
  }

  get registrosFin(): number {
    return Math.min(this.pagina * this.pageSize, this.totalFiltrado);
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

  // COLOR VISUAL
  obtenerColor(color: string): string {
    const colores: Record<string, string> = {
      'Blanco': '#FFFFFF',
      'Negro': '#222222',
      'Azul': '#357ABD',
      'Verde': '#8BC34A',
      'Rojo': '#E57373',
      'Transparente': '#E5E7EB'
    };

    return colores[color] || '#90A4AE';
  }

  // ACCIONES
  ModalRegistrarProducto(): void {
    this.toastr.info(
      'Aquí se abrirá el formulario para registrar un producto.',
      'Registrar producto'
    );
  }

  editarInventario(item: Inventario): void {
    this.toastr.info(
      `Editar producto ${item.referencia}.`,
      'Editar inventario'
    );
  }

  eliminarInventario(id: number): void {
    const confirmar = window.confirm(
      '¿Estás seguro de eliminar este producto del inventario?'
    );

    if (!confirmar) {
      return;
    }

    this.inventario = this.inventario.filter(item => item.id !== id);

    this.aplicarFiltros();

    this.toastr.success(
      'El producto fue eliminado correctamente.',
      'Inventario'
    );
  }

  recalcular(): void {
    if (this.cargando) {
      return;
    }

    this.cargando = true;

    // Simulación temporal
    setTimeout(() => {
      this.cargando = false;

      this.toastr.success(
        'El inventario fue recalculado correctamente.',
        'Inventario'
      );
    }, 1200);
  }
}
