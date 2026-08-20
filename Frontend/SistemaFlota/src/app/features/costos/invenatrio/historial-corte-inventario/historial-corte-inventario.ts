import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CorteInventarioService } from '../../../../core/services/costos/inventario/cortesinventario/corteinventario.service';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { CorteInventarioHistorial } from '../../../../core/models/costos/inventario/historialcorteinventario/historialcorteInventario.models';
import { DetalleCorteInventario } from '../detalle-corte-inventario/detalle-corte-inventario';

@Component({
  selector: 'app-historial-corte-inventario',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './historial-corte-inventario.html',
  styleUrl: './historial-corte-inventario.scss',
})
export class HistorialCorteInventario implements OnInit {

  cortes: CorteInventarioHistorial[] = [];

  constructor(
    private corteService: CorteInventarioService,
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
        this.toastr.error(
          'No se pudo cargar el historial de cortes',
          'Error'
        );
      }
    });
  }

  verDetalle(id: number): void {
    this.corteService.obtenerDetalle(id).subscribe({
      next: (data) => {
        this.dialog.open(DetalleCorteInventario, {
          width: '1000px',
          maxWidth: '95vw',
          data: data
        });
      },
      error: (err) => {
        console.error(err);
        this.toastr.error('No se pudo cargar el detalle del corte', 'Error');
      }
    });
  }
}