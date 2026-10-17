import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface ProveedorResumen {
  proveedor: string;
  kgTotal: number;
  bultos: number;
  valorTotal: number;
  porcentajeInventario: number;
}

@Component({
  selector: 'app-dashboard-proveedores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-proveedores.html',
  styleUrl: './dashboard-proveedores.scss',
})
export class DashboardProveedores {

  @Input() proveedoresPaginados: ProveedorResumen[] = [];
  @Input() supplierSummaryList: ProveedorResumen[] = [];

  @Input() totalKg = 0;
  @Input() totalBultos = 0;
  @Input() totalProveedores = 0;
  @Input() totalValor = 0;

  @Input() paginaProveedores = 1;
  @Input() totalPaginasProveedores = 1;

  @Output() cambiarPagina = new EventEmitter<number>();

  cambiarPaginaProveedores(direccion: number): void {
    this.cambiarPagina.emit(direccion);
  }
}
