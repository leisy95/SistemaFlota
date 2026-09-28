import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { MaterialNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/materiales/material-no-formalizado.service';
import { ProveedorNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/proveedores/proveedor-no-formalizado.service';
import { CrearOrdenCompraNoFormalizadaRequest } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/crearordencompra-no-formalizada.model';
import { ProveedorNoFormalizado } from '../../../../core/models/compras-no-formalizadas/proveedores/proveedor-no-formalizado.model';
import { MaterialNoFormalizado } from '../../../../core/models/compras-no-formalizadas/materiales/material-no-formalizado.model';
import { ItemOrdenNoFormalizada } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/item-orden-no-formalizada.model';
import { OrdenCompraNoFormalizadaResponse } from '../../../../core/models/compras-no-formalizadas/ordenes-compras/ordencompra-no-formalizada-response.model';
import { OrdenCompraNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-compras/OrdenCompraNoFormalizadaService';

@Component({
  selector: 'app-crear-orden-compra',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule
  ],
  templateUrl: './crear-orden-compra.html',
  styleUrl: './crear-orden-compra.scss',
})
export class CrearOrdenCompra implements OnInit {
  constructor(
    private dialogRef: MatDialogRef<CrearOrdenCompra>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private ordenCompraService: OrdenCompraNoFormalizadaService,
    private proveedorService: ProveedorNoFormalizadoService,
    private materialService: MaterialNoFormalizadoService,
    private toastr: ToastrService
  ) { }

  formasPago: string[] = [
    'Contado',
    '50% Anticipo / 50% Contra entrega',
    'Crédito',
    'Crédito 15 días',
    'Crédito 30 días',
    'Crédito 60 días',
    'Mercancía puesta en nuestras instalaciones'
  ];

  orden: CrearOrdenCompraNoFormalizadaRequest = {
    id: undefined,
    proveedorNoFormalizadoId: 0,
    formaPago: 'Contado',
    fechaOrden: '',
    fechaEntrega: '',
    lugarEntrega: 'Mercancía puesta en nuestras instalaciones',
    tipoImpuesto: 'IVA',
    porcentajeImpuesto: 19,
    observaciones: '',
    detalles: []
  };

  proveedores: ProveedorNoFormalizado[] = [];
  materiales: MaterialNoFormalizado[] = [];
  items: ItemOrdenNoFormalizada[] = [];

  totalKg = 0;
  totalBultos = 0;
  subtotal = 0;
  valorImpuesto = 0;
  totalPagar = 0;

  modoEditar = false;
  mostrarErrores = false;
  guardando = false;

  ngOnInit(): void {
    this.modoEditar = this.data?.modo === 'editar';

    if (!this.modoEditar) {
      this.orden.fechaOrden = this.obtenerFechaHoy();
      this.orden.fechaEntrega = this.obtenerFechaMasUnMes();
    }

    this.obtenerProveedores();

    if (this.modoEditar) {
      this.cargarOrden(this.data.id);
    } else {
      this.items.push(this.nuevoItem());
    }
  }

  private obtenerFechaHoy(): string {
    return new Date().toISOString().substring(0, 10);
  }

  private obtenerFechaMasUnMes(): string {
    const fecha = new Date();
    fecha.setMonth(fecha.getMonth() + 1);
    return fecha.toISOString().substring(0, 10);
  }

  obtenerProveedores(): void {
    this.proveedorService
      .obtener('', '', 'nombre', 1, 1000)
      .subscribe({
        next: respuesta => {
          this.proveedores = respuesta.datos;
        },
        error: () => {
          this.toastr.error(
            'No fue posible cargar los proveedores no formalizados.',
            'Error'
          );
        }
      });
  }

  obtenerMaterialesPorProveedor(proveedorNoFormalizadoId: number): void {
    if (!proveedorNoFormalizadoId || proveedorNoFormalizadoId === 0) {
      this.materiales = [];
      return;
    }

    this.materialService
      .obtener(
        '',
        '',
        '',
        proveedorNoFormalizadoId.toString(),
        '',
        1,
        1000
      )
      .subscribe({
        next: respuesta => {
          this.materiales = respuesta.datos;
        },
        error: () => {
          this.materiales = [];
          this.toastr.error(
            'No fue posible cargar los materiales del proveedor.',
            'Error'
          );
        }
      });
  }

  nuevoItem(): ItemOrdenNoFormalizada {
    return {
      materialNoFormalizadoId: 0,
      color: '',
      kilos: 0,
      kgBulto: 25,
      bultos: 0,
      costo: 0,
      subtotal: 0
    };
  }

  seleccionarMaterial(index: number, materialNoFormalizadoId: number): void {
    const item = this.items[index];

    const material = this.materiales.find(
      m => m.idMaterialNoFormalizado === materialNoFormalizadoId
    );

    if (!material) {
      item.color = '';
      return;
    }

    item.color = material.color ?? '';
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
      this.subtotal * (this.orden.porcentajeImpuesto / 100);
    this.totalPagar = this.subtotal + this.valorImpuesto;
  }

  cargarOrden(id: number): void {
    this.ordenCompraService
      .obtenerPorId(id)
      .subscribe({
        next: (orden: OrdenCompraNoFormalizadaResponse) => {
          this.orden = {
            id: orden.id,
            proveedorNoFormalizadoId: orden.proveedorNoFormalizadoId,
            formaPago: orden.formaPago,
            fechaOrden: orden.fechaOrden.substring(0, 10),
            fechaEntrega: orden.fechaEntrega
              ? orden.fechaEntrega.substring(0, 10)
              : '',
            lugarEntrega: orden.lugarEntrega,
            tipoImpuesto: orden.tipoImpuesto,
            porcentajeImpuesto: orden.porcentajeImpuesto,
            observaciones: orden.observaciones,
            detalles: []
          };

          this.obtenerMaterialesPorProveedor(
            this.orden.proveedorNoFormalizadoId
          );

          this.items = orden.detalles.map(detalle => ({
            materialNoFormalizadoId: detalle.materialNoFormalizadoId,
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
      materialNoFormalizadoId: item.materialNoFormalizadoId,
      color: item.color,
      cantidadKg: item.kilos,
      kgPorBulto: item.kgBulto,
      bultos: item.bultos,
      costoKg: item.costo,
      subtotal: item.subtotal
    }));
  }

  validarFormulario(): boolean {
    this.mostrarErrores = true;

    if (!this.orden.proveedorNoFormalizadoId) return false;
    if (!this.orden.formaPago?.trim()) return false;
    if (!this.orden.fechaOrden) return false;
    if (!this.orden.fechaEntrega) return false;
    if (this.orden.fechaEntrega < this.orden.fechaOrden) return false;
    if (!this.orden.lugarEntrega?.trim()) return false;

    const impuesto = Number(this.orden.porcentajeImpuesto);

    if (impuesto < 0 || impuesto > 100)
      return false;

    if (this.items.length === 0)
      return false;

    for (const item of this.items) {
      if (!item.materialNoFormalizadoId) return false;
      if (item.kilos <= 0) return false;
      if (item.kgBulto <= 0) return false;
      if (item.costo <= 0) return false;
    }

    return true;
  }

  guardarOrden(): void {
    if (!this.validarFormulario()) {
      this.toastr.warning(
        'Revise los campos marcados antes de guardar.',
        'Formulario incompleto'
      );
      return;
    }

    if (this.guardando)
      return;

    this.guardando = true;
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

            this.dialogRef.close({
              actualizado: true
            });
          },
          error: () => {
            this.guardando = false;
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
          next: respuesta => {
            this.toastr.success(
              `Orden ${respuesta.numero} creada correctamente.`,
              'Éxito'
            );

            this.dialogRef.close(true);
          },
          error: () => {
            this.guardando = false;
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
