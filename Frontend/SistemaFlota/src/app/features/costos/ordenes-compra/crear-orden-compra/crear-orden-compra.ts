import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ProveedorService } from '../../../../core/services/costos/proveedores/proveedor.service';
import { MaterialService } from '../../../../core/services/costos/materiales/materiales.service';
import { Proveedor } from '../../../../core/models/costos/proveedores/proveedores.model';
import { Material } from '../../../../core/models/costos/materiales/material.models';
import { ItemOrden } from '../../../../core/models/costos/ordenCompra/item-orden.model';
import { OrdenCompraService } from '../../../../core/services/costos/ordencompra/ordencompra.service';
import { CrearOrdenCompraRequest } from '../../../../core/models/costos/ordenCompra/crearordencompra.model';
import { OrdenCompraResponse } from '../../../../core/models/costos/ordenCompra/ordencompra-response.model';

@Component({
  selector: 'app-crear-orden-compra',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './crear-orden-compra.html',
  styleUrl: './crear-orden-compra.scss'
})
export class CrearOrdenCompra implements OnInit {

  constructor(
    private dialogRef: MatDialogRef<CrearOrdenCompra>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private ordenCompraService: OrdenCompraService,
    private proveedorService: ProveedorService,
    private materialService: MaterialService,
    private toastr: ToastrService
  ) { }

  formasPago: string[] = [
    'Contado',
    '50% Anticipo / 50% Contra entrega',
    'Crédito 15 días',
    'Crédito 30 días',
    'Crédito 60 días',
    'Mercancía puesta en nuestras instalaciones'
  ];

  orden: CrearOrdenCompraRequest = {
    id: undefined,

    proveedorId: 0,
    formaPago: 'Contado',
    fechaOrden: new Date().toISOString().substring(0, 10),
    fechaEntrega: new Date().toISOString().substring(0, 10),

    lugarEntrega: 'Mercancía puesta en nuestras instalaciones',
    tipoImpuesto: 'IVA',
    porcentajeImpuesto: 19,

    observaciones: '',
    detalles: []
  };

  proveedores: Proveedor[] = [];
  materiales: Material[] = [];
  items: ItemOrden[] = [];

  totalKg = 0;
  totalBultos = 0;
  subtotal = 0;
  valorImpuesto = 0;
  totalPagar = 0;

  modoEditar = false;

  ngOnInit(): void {
    this.modoEditar = this.data?.modo === 'editar';
    this.obtenerProveedores();
    this.obtenerMateriales();

    if (this.modoEditar) {
      this.cargarOrden(this.data.id);

    } else {
      this.items.push(this.nuevoItem());
    }
  }


  obtenerProveedores(): void {

    this.proveedorService
      .obtener('', '', 'nombre', 1, 1000)
      .subscribe({

        next: (respuesta) => {
          this.proveedores = respuesta.datos;
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar los proveedores.',
            'Error'
          );
        }
      });
  }

  obtenerMateriales(): void {

    this.materialService
      .obtener('', '', '', '', '', 1, 1000)
      .subscribe({

        next: (respuesta) => {
          this.materiales = respuesta.datos;
        },
        error: () => {
          this.toastr.error(
            'No fue posible cargar los materiales.',
            'Error'
          );
        }
      });
  }

  nuevoItem(): ItemOrden {

    return {
      materialId: 0,
      color: '',
      kilos: 0,
      kgBulto: 25,
      bultos: 0,
      costo: 0,
      subtotal: 0
    };
  }

  agregarItem(): void {

    this.items.push(this.nuevoItem());
    this.actualizarTotales();
  }

  eliminarItem(index: number): void {
    this.items.splice(index, 1);
    this.actualizarTotales();
  }

  calcular(index: number): void {

    const item = this.items[index];
    item.bultos =
      item.kgBulto > 0
        ? Math.ceil(item.kilos / item.kgBulto)
        : 0;

    item.subtotal = item.kilos * item.costo;
    this.actualizarTotales();
  }

  actualizarTotales(): void {

    this.totalKg = this.items.reduce((s, x) => s + x.kilos, 0);
    this.totalBultos = this.items.reduce((s, x) => s + x.bultos, 0);
    this.subtotal = this.items.reduce((s, x) => s + x.subtotal, 0);
    this.valorImpuesto =
      this.subtotal *
      (this.orden.porcentajeImpuesto / 100);

    this.totalPagar =
      this.subtotal +
      this.valorImpuesto;
  }

  cargarOrden(id: number): void {

    this.ordenCompraService
      .obtenerPorId(id)
      .subscribe({

        next: (

          orden: OrdenCompraResponse) => {

          this.orden = {
            id: orden.id,
            proveedorId: orden.proveedorId,
            formaPago: orden.formaPago,
            fechaOrden: orden.fechaOrden.substring(0, 10),
            fechaEntrega: orden.fechaEntrega.substring(0, 10),
            lugarEntrega: orden.lugarEntrega,
            tipoImpuesto: orden.tipoImpuesto,
            porcentajeImpuesto: orden.porcentajeImpuesto,
            observaciones: orden.observaciones,
            detalles: orden.detalles ?? []
          };


          this.items = orden.detalles.map(detalle => ({
            materialId: detalle.materialId,
            color: detalle.color,
            kilos: detalle.cantidadKg,
            kgBulto: detalle.kgPorBulto,
            bultos: detalle.bultos,
            costo: detalle.costoKg,
            subtotal: detalle.subtotal
          }));

          this.actualizarTotales();
        },

        error: () => {
          this.toastr.error(
            'No fue posible cargar la orden.',
            'Error'
          );
        }
      });
  }

  private prepararDetalles(): void {

    this.orden.detalles = this.items.map(item => ({
      materialId: item.materialId,
      color: item.color,
      cantidadKg: item.kilos,
      kgPorBulto: item.kgBulto,
      bultos: item.bultos,
      costoKg: item.costo,
      subtotal: item.subtotal
    }));

  }

  guardarOrden(): void {

    this.prepararDetalles();

    if (this.modoEditar) {

      this.ordenCompraService
        .actualizar(this.orden.id!, this.orden)
        .subscribe({
          next: () => {
            this.toastr.success(
              'La orden fue actualizada correctamente.',
              'Éxito'
            );
            this.dialogRef.close(true);
          },
          error: () => {
            this.toastr.error(
              'No fue posible actualizar la orden.',
              'Error'
            );
          }
        });

    } else {

      this.ordenCompraService
        .crear(this.orden)
        .subscribe({
          next: (respuesta) => {
            this.toastr.success(
              `Orden ${respuesta.numero} creada correctamente.`,
              'Éxito'
            );
            this.dialogRef.close(true);
          },
          error: () => {
            this.toastr.error(
              'No fue posible crear la orden.',
              'Error'
            );
          }
        });

    }
  }

  generarPdf() {

    if (!this.orden.id) {
      alert('Primero debe guardar la orden.');
      return;
    }

    this.ordenCompraService
      .generarPdf(this.orden.id)
      .subscribe({
        next: (blob) => {

          const url = window.URL.createObjectURL(blob);
          window.open(url, '_blank');
        },
        error: err => {
          console.error(err);
        }
      });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}