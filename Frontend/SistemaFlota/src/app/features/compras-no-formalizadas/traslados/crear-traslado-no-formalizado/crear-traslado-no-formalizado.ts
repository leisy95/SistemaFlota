import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';

import { InventarioNoFormalizado } from '../../../../core/models/compras-no-formalizadas/inventario/inventario-no-formalizado.model';
import { CrearOrdenTrasladoNoFormalizadaDetalle } from '../../../../core/models/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizado.models';
import { OrdenTrasladoNoFormalizadaService } from '../../../../core/services/compras-no-formalizadas/ordenes-traslado/orden-traslado-no-formalizada.service';
import { InventarioNoFormalizadoService } from '../../../../core/services/compras-no-formalizadas/inventario/inventario-no-formalizado.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-crear-traslado-no-formalizado',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './crear-traslado-no-formalizado.html',
  styleUrl: './crear-traslado-no-formalizado.scss',
})
export class CrearTrasladoNoFormalizado {

  fecha = new Date().toLocaleDateString('es-CO');
  destino = 'Extrusión';
  usuario = '';
  guardando = false;
  cargandoInventario = false;

  inventarios: InventarioNoFormalizado[] = [];
  materialesDisponibles: InventarioNoFormalizado[] = [];

  proveedores: string[] = [];
  tiposMaterial: string[] = [];
  densidades: string[] = [];
  colores: string[] = [];

  materialActual: CrearOrdenTrasladoNoFormalizadaDetalle = {
    materialNoFormalizadoId: null,
    material: '',
    proveedor: '',
    tipo: '',
    densidad: '',
    color: '',
    cantidadKg: 0,
    bultos: 0
  };

  materiales: CrearOrdenTrasladoNoFormalizadaDetalle[] = [];

  constructor(
    private ordenTrasladoService: OrdenTrasladoNoFormalizadaService,
    private authService: AuthService,
    private inventarioService: InventarioNoFormalizadoService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<CrearTrasladoNoFormalizado>
  ) {
    this.usuario = this.authService.username;
  }

  ngOnInit(): void {
    this.cargarInventario();
  }

  cargarInventario(): void {
    this.cargandoInventario = true;

    this.inventarioService
      .obtener(undefined, null, null, null, 1, 1000)
      .subscribe({
        next: (respuesta) => {
          this.inventarios = respuesta.items;
          this.cargarOpciones();
          this.cargandoInventario = false;
        },
        error: () => {
          this.cargandoInventario = false;
          this.toastr.error(
            'No fue posible cargar el inventario no formalizado.',
            'Error'
          );
        }
      });
  }

  cambiarProveedor(): void {
    this.materialActual.materialNoFormalizadoId = null;
    this.materialActual.material = '';
    this.materialActual.tipo = '';
    this.materialActual.densidad = '';
    this.materialActual.color = '';
    this.materialActual.cantidadKg = 0;
    this.materialActual.bultos = 0;
  }

  cargarOpciones(): void {
    this.proveedores = [
      ...new Set(
        this.inventarios
          .map(x => x.proveedor)
          .filter(x => !!x)
      )
    ];

    this.tiposMaterial = [
      ...new Set(
        this.inventarios
          .map(x => x.categoria)
          .filter(x => !!x)
      )
    ];

    this.densidades = [
      ...new Set(
        this.inventarios
          .map(x => x.densidad)
          .filter(x => !!x)
      )
    ];

    this.colores = [
      ...new Set(
        this.inventarios
          .map(x => x.color)
          .filter(x => !!x)
      )
    ];
  }

  get inventariosFiltrados(): InventarioNoFormalizado[] {
    return this.inventarios.filter(x =>
      (!this.materialActual.proveedor ||
        x.proveedor === this.materialActual.proveedor) &&

      (!this.materialActual.tipo ||
        x.categoria === this.materialActual.tipo) &&

      (!this.materialActual.densidad ||
        x.densidad === this.materialActual.densidad) &&

      (!this.materialActual.color ||
        x.color === this.materialActual.color)
    );
  }

  seleccionarMaterial(): void {
    if (!this.materialActual.materialNoFormalizadoId) {
      return;
    }

    const inventario = this.inventarios.find(
      x =>
        x.materialId ===
        this.materialActual.materialNoFormalizadoId
    );

    if (!inventario) {
      return;
    }

    this.materialActual.material =
      inventario.material || inventario.categoria;

    this.materialActual.proveedor =
      inventario.proveedor;

    this.materialActual.tipo =
      inventario.categoria;

    this.materialActual.densidad =
      inventario.densidad;

    this.materialActual.color =
      inventario.color;

    this.materialActual.cantidadKg = 0;
    this.materialActual.bultos = 0;
  }

  get inventarioSeleccionado(): InventarioNoFormalizado | undefined {
    if (!this.materialActual.materialNoFormalizadoId) {
      return undefined;
    }

    return this.inventarios.find(
      x =>
        x.materialId ===
        this.materialActual.materialNoFormalizadoId
    );
  }

  get stockActual(): number {
    return Number(
      this.inventarioSeleccionado?.stockActual ?? 0
    );
  }

  get cantidadComprometida(): number {
    return Number(
      this.inventarioSeleccionado?.cantidadComprometida ?? 0
    );
  }

  get stockDisponible(): number {
    return Number(
      this.inventarioSeleccionado?.stockDisponible ?? 0
    );
  }

  get cantidadSuperaStock(): boolean {
    return Number(
      this.materialActual.cantidadKg ?? 0
    ) > this.stockDisponible;
  }

  get totalKg(): number {
    return this.materiales.reduce(
      (total, material) =>
        total + Number(material.cantidadKg),
      0
    );
  }

  get totalBultos(): number {
    return this.materiales.reduce(
      (total, material) =>
        total + Number(material.bultos),
      0
    );
  }

  agregarMaterial(): void {

    if (
      !this.materialActual.proveedor ||
      !this.materialActual.tipo ||
      !this.materialActual.densidad ||
      !this.materialActual.color
    ) {
      this.toastr.warning(
        'Complete todos los datos del material.',
        'Datos incompletos'
      );
      return;
    }

    if (!this.materialActual.materialNoFormalizadoId) {
      this.toastr.error(
        'Seleccione un material válido del inventario no formalizado.',
        'Material inválido'
      );
      return;
    }

    const cantidadKg =
      Number(this.materialActual.cantidadKg);

    const bultos =
      Number(this.materialActual.bultos);

    if (!cantidadKg || cantidadKg <= 0) {
      this.toastr.warning(
        'La cantidad debe ser mayor a cero.',
        'Cantidad inválida'
      );
      return;
    }

    if (!bultos || bultos <= 0) {
      this.toastr.warning(
        'La cantidad de bultos debe ser mayor a cero.',
        'Bultos inválidos'
      );
      return;
    }

    if (!Number.isInteger(bultos)) {
      this.toastr.warning(
        'La cantidad de bultos debe ser un número entero.',
        'Bultos inválidos'
      );
      return;
    }

    if (cantidadKg > this.stockDisponible) {
      this.toastr.error(
        `Solo hay ${this.stockDisponible.toLocaleString('es-CO')} KG disponibles para este material.`,
        'Stock insuficiente'
      );
      return;
    }

    const cantidadYaSolicitada = this.materiales
      .filter(x =>
        x.materialNoFormalizadoId ===
        this.materialActual.materialNoFormalizadoId &&
        x.color === this.materialActual.color
      )
      .reduce(
        (total, x) =>
          total + Number(x.cantidadKg),
        0
      );

    const disponibleDespues =
      this.stockDisponible -
      cantidadYaSolicitada;

    if (cantidadKg > disponibleDespues) {
      this.toastr.error(
        `Ya tienes ${cantidadYaSolicitada.toLocaleString('es-CO')} KG solicitados. Solo puedes agregar ${Math.max(
          disponibleDespues,
          0
        ).toLocaleString('es-CO')} KG adicionales.`,
        'Stock insuficiente'
      );
      return;
    }

    this.materiales.push({
      ...this.materialActual,
      cantidadKg,
      bultos
    });

    this.limpiarMaterial();
  }

  limpiarMaterial(): void {
    this.materialActual = {
      materialNoFormalizadoId: null,
      material: '',
      proveedor: '',
      tipo: '',
      densidad: '',
      color: '',
      cantidadKg: 0,
      bultos: 0
    };
  }

  eliminarMaterial(index: number): void {
    this.materiales.splice(index, 1);
  }

  generarOrden(): void {

    if (this.materiales.length === 0) {
      this.toastr.warning(
        'Debe agregar al menos un material.',
        'Orden de traslado'
      );
      return;
    }

    const dto = {
      destino: this.destino,
      materiales: this.materiales
    };

    this.guardando = true;

    this.ordenTrasladoService.crear(dto).subscribe({
      next: (respuesta) => {
        this.guardando = false;

        this.toastr.success(
          `Orden ${respuesta.numeroOrden} generada correctamente.`,
          'Orden de traslado'
        );

        this.dialogRef.close(respuesta);
      },

      error: (error) => {
        this.guardando = false;

        this.toastr.error(
          error?.error?.mensaje ||
          error?.error?.message ||
          'No fue posible generar la orden de traslado.',
          'Error'
        );
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}