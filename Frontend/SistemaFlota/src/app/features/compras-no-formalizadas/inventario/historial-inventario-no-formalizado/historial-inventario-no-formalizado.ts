import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { AjusteInventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/ajuste-inventario-no-formalizado.service';

@Component({
  selector: 'app-historial-inventario-no-formalizado',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './historial-inventario-no-formalizado.html',
  styleUrl: './historial-inventario-no-formalizado.scss'
})
export class HistorialInventarioNoFormalizado implements OnInit {
  historial: any[] = [];

  constructor(
    private service: AjusteInventarioNoFormalizadoService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<HistorialInventarioNoFormalizado>,
    @Inject(MAT_DIALOG_DATA) public inventarioId: number
  ) { }

  ngOnInit(): void {
    this.cargarHistorial();
  }

  cargarHistorial(): void {
    this.service.obtenerHistorial(this.inventarioId).subscribe({
      next: resp => this.historial = resp,
      error: () => {
        this.toastr.error('No fue posible cargar el historial del inventario no formalizado.');
        this.dialogRef.close();
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}
