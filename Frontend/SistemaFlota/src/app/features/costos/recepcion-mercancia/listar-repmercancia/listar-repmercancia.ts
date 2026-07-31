import { CommonModule, DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { IniciarRepmercancia } from '../iniciar-repmercancia/iniciar-repmercancia';
import { OrdenCompra } from '../../../../core/models/costos/ordenCompra/ordencompra.model';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';

@Component({
  selector: 'app-listar-repmercancia',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DecimalPipe
  ],
  templateUrl: './listar-repmercancia.html',
  styleUrl: './listar-repmercancia.scss',
})
export class ListarRepmercancia {

  pagina = 1;
  pageSize = 10;
  total = 0;

  buscar = '';
  ordenSeleccionada: any = null;

  ordenes: OrdenCompra[] = [];

  constructor(
    private toastr: ToastrService,
    private dialog: MatDialog,
    private ordenCompraService: OrdenCompraService
  ) { }

  ngOnInit(): void {
    this.cargarOrdenes();
  }

  cargarOrdenes(): void {
    this.ordenCompraService.obtener(
      this.pagina,
      this.pageSize,
      this.buscar
    ).subscribe({
      next: (resp) => {
        this.ordenes = resp.items;
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar las órdenes.',
          'Recepción'
        );
      }
    });
  }

  buscarOrden() {
    this.ordenSeleccionada = null;
    this.cargarOrdenes();
  }

  get totalPaginas(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  cambiarPagina(pagina: number) {

    if (pagina < 1 || pagina > this.totalPaginas) {
      return;
    }

    this.pagina = pagina;
    this.cargarOrdenes();
  }

  abrirOrden(orden: any) {

    this.ordenSeleccionada = orden;

    this.toastr.success(
      `Orden ${orden.numero} seleccionada`,
      'Recepción'
    );

  }

  iniciarRecepcion(): void {

    if (!this.ordenSeleccionada) {
      this.toastr.warning(
        'Seleccione una orden.',
        'Recepción'
      );
      return;
    }

    const dialogRef = this.dialog.open(IniciarRepmercancia, {
      width: '1200px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      disableClose: true,
      autoFocus: false,
      data: this.ordenSeleccionada
    });

    dialogRef.afterClosed().subscribe(resultado => {

      if (!resultado) {
        return;
      }

      console.log(resultado);

      this.toastr.success(
        'Recepción registrada correctamente.',
        'Recepción'
      );
    });
  }

}
