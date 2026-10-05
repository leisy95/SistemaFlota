import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { CorteInventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/cortesinvenatrio/corte-inventario-no-formalizado.service';
import { DetalleCorteInventarioNoFormalizado } from '../detalle-corte-inventario-no-formalizado/detalle-corte-inventario-no-formalizado';
import { CorteInventarioHistorialNoFormalizado } from '../../../../core/models/compras-no-formalizadas/inventario/historial-corte-inventario/corte-inventario-historial-no-formalizado.model';

@Component({
  selector: 'app-historial-corte-inventario-no-formalizado',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './historial-corte-inventario-no-formalizado.html',
  styleUrl: './historial-corte-inventario-no-formalizado.scss'
})
export class HistorialCorteInventarioNoFormalizado implements OnInit {
  cortes: CorteInventarioHistorialNoFormalizado[] = [];

  constructor(
    private corteService: CorteInventarioNoFormalizadoService,
    private toastr: ToastrService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.cargarHistorial();
  }

  cargarHistorial(): void {
    this.corteService.obtenerHistorial().subscribe({
      next: (data) => {
        this.cortes = data;
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('No se pudo cargar el historial de cortes no formalizados', 'Error');
      }
    });
  }

  verDetalle(id: number): void {
    this.corteService.obtenerDetalle(id).subscribe({
      next: (data) => {
        this.dialog.open(DetalleCorteInventarioNoFormalizado, {
          width: '1000px',
          maxWidth: '95vw',
          data
        });
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('No se pudo cargar el detalle del corte no formalizado', 'Error');
      }
    });
  }
}
