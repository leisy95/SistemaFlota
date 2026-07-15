import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { MatDialog } from '@angular/material/dialog';
import { CrearMateriales } from '../crear-materiales/crear-materiales';

@Component({
  selector: 'app-listar-materiales',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './listar-materiales.html',
  styleUrl: './listar-materiales.scss',
})
export class ListarMateriales {

  buscar = '';

  materiales = [
    {
      codigo: 'R0001',
      proveedor: 'Plásticos del Norte',
      material: 'Polietileno',
      descripcion: 'Dow DFDA-1737 - Soplado',
      densidad: 'PEBD',
      categoria: 'MP Original',
      tipo: 'Almacenable',
      color: 'Natural',
      linea: 'Línea A',
      precio: 48500,
      bultos: 50,
      Unidad: 'Kilogramos',
      cantidad: 1250,
      fecha: '14/07/2026'
    },
    {
      codigo: 'R0002',
      proveedor: 'Gecobags',
      material: 'Polipropileno',
      descripcion: 'Fea DFDA-1737 - Arrugado',
      densidad: 'PEAD',
      categoria: 'MP Original',
      tipo: 'Almacenable',
      color: 'Negro',
      linea: 'Línea B',
      precio: 39200,
      bultos: 35,
      Unidad: 'Kilogramos',
      cantidad: 980,
      fecha: '13/07/2026'
    },
    {
      codigo: 'R0003',
      proveedor: 'Resinplas',
      material: 'PVC',
      descripcion: 'Gae DFDA-1737 - Liso',
      densidad: 'PVC',
      categoria: 'MP Post-Industrial',
      tipo: 'Consumible',
      color: 'Blanco',
      linea: 'Línea C',
      precio: 52800,
      bultos: 40,
      Unidad: 'Kilogramos',
      cantidad: 1500,
      fecha: '12/07/2026'
    }
  ];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog
  ) { }

  nuevoMaterial(): void {
    const dialogRef = this.dialog.open(CrearMateriales, {
      width: '700px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(resultado => {
      if (resultado) {
        // Recargar la lista o guardar el proveedor
      }
    });
  }

  editar(material: any): void {
    this.dialog.open(CrearMateriales, {
      width: '700px',
      data: material
    });
  }

  eliminar(material: any): void {
    console.log('Eliminar:', material);
  }

}