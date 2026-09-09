import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { CrearOrdenTrasladoDetalle } from '../../../../core/models/costos/OrdenesTraslado/orden-traslado.model';
import { OrdenTrasladoService } from '../../../../core/services/costos/ordenestraslado/ordentraslado.service';
import { AuthService } from '../../../../core/services/auth.service';
import { Inventario_Costos } from '../../../../core/models/costos/inventario/inventario-costos.models';
import { InventarioService } from '../../../../core/services/costos/inventario/inventario.service';

@Component({
  selector: 'app-crear-traslado',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule],
  templateUrl: './crear-traslado.html',
  styleUrl: './crear-traslado.scss',
})
export class CrearTraslado {
  fecha = new Date().toLocaleDateString('es-CO');
  destino = 'Extrusión';
  usuario = '';
  guardando = false;
  cargandoInventario = false;

  inventarios: Inventario_Costos[] = [];

  proveedores: string[] = [];
  tiposMaterial: string[] = [];
  densidades: string[] = [];
  colores: string[] = [];

  materialActual: CrearOrdenTrasladoDetalle = {
    materialId: null,
    proveedor: '',
    tipoProduccion: '',
    densidad: '',
    color: '',
    cantidadKg: 0,
    bultos: 0
  };

  materiales: CrearOrdenTrasladoDetalle[] = [];

  constructor(
    private ordenTrasladoService: OrdenTrasladoService,
    private authService: AuthService,
    private inventarioService: InventarioService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<CrearTraslado>
  ) {
    this.usuario = this.authService.username;
  }

  ngOnInit(): void {
    this.cargarInventario();
  }

  cargarInventario(): void {
    this.cargandoInventario = true;

    this.inventarioService.obtener(
      undefined,
      null,
      null,
      null,
      1,
      1000
    ).subscribe({
      next: (respuesta) => {
        this.inventarios = respuesta.items ?? respuesta.data ?? respuesta;

        this.cargarOpciones();

        this.cargandoInventario = false;
      },
      error: () => {
        this.cargandoInventario = false;

        this.toastr.error(
          'No fue posible cargar el inventario.',
          'Error'
        );
      }
    });
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
          .map(x => x.tipoProduccion)
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

  get inventariosFiltrados(): Inventario_Costos[] {
    return this.inventarios.filter(x =>
      (!this.materialActual.proveedor ||
        x.proveedor === this.materialActual.proveedor) &&
      (!this.materialActual.tipoProduccion ||
        x.tipoProduccion === this.materialActual.tipoProduccion) &&
      (!this.materialActual.densidad ||
        x.densidad === this.materialActual.densidad) &&
      (!this.materialActual.color ||
        x.color === this.materialActual.color)
    );
  }

  seleccionarMaterial(): void {

    const inventarios = this.inventariosFiltrados;

    if (inventarios.length === 0) {
      this.materialActual.materialId = null;
      return;
    }

    if (inventarios.length > 1) {
      this.materialActual.materialId = null;

      return;
    }

    const inventario = inventarios[0];

    this.materialActual.materialId = inventario.materialId;

    this.materialActual.proveedor = inventario.proveedor;
    this.materialActual.tipoProduccion = inventario.tipoProduccion;
    this.materialActual.densidad = inventario.densidad;
    this.materialActual.color = inventario.color;
  }

  get totalKg(): number {
    return this.materiales.reduce(
      (total, material) => total + Number(material.cantidadKg),
      0
    );
  }

  get totalBultos(): number {
    return this.materiales.reduce(
      (total, material) => total + Number(material.bultos),
      0
    );
  }

  agregarMaterial(): void {

    if (
      !this.materialActual.proveedor ||
      !this.materialActual.tipoProduccion ||
      !this.materialActual.densidad ||
      !this.materialActual.color
    ) {

      this.toastr.warning(
        'Complete todos los datos del material.',
        'Datos incompletos'
      );

      return;
    }

    if (!this.materialActual.materialId) {

      this.toastr.error(
        'Seleccione un material válido del inventario.',
        'Material inválido'
      );

      return;
    }

    const cantidadKg = Number(this.materialActual.cantidadKg);

    if (!cantidadKg || cantidadKg <= 0) {

      this.toastr.warning(
        'La cantidad debe ser mayor a cero.',
        'Cantidad inválida'
      );

      return;
    }

    // Cada bulto pesa 25 KG
    if (cantidadKg % 25 !== 0) {

      this.toastr.warning(
        'La cantidad debe ser múltiplo de 25 kg.',
        'Cantidad inválida'
      );

      return;
    }

    // Calcular bultos automáticamente
    const bultos = cantidadKg / 25;

    this.materiales.push({
      ...this.materialActual,
      cantidadKg: cantidadKg,
      bultos: bultos
    });

    // Limpiar formulario
    this.materialActual = {
      materialId: null,
      proveedor: '',
      tipoProduccion: '',
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
      this.toastr.warning('Debe agregar al menos un material.', 'Orden de traslado');
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
          error?.error?.mensaje || 'No fue posible generar la orden de traslado.',
          'Error'
        );
      }
    });
  }

  cerrar(): void {
    this.dialogRef.close();
  }
}