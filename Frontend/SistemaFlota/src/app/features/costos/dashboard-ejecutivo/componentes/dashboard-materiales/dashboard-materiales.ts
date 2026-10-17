import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface MaterialCosto {
  material: string;
  proveedor: string;
  kg: number;
  costoKg: number;
  valor: number;
}

@Component({
  selector: 'app-dashboard-materiales',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-materiales.html',
  styleUrl: './dashboard-materiales.scss',
})
export class DashboardMateriales {

  @Input() materialesPaginados: MaterialCosto[] = [];
  @Input() cargandoMateriales = false;
  @Input() paginaMateriales = 1;
  @Input() totalPaginasMateriales = 1;
  @Input() totalMaterialesMayorCosto = 0;

  @Output() cambiarPagina = new EventEmitter<number>();

  cambiarPaginaMateriales(direccion: number): void {
    this.cambiarPagina.emit(direccion);
  }
}
