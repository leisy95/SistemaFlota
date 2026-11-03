import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { OrdenCompra } from '../../../../core/models/costos/ordenCompra/ordencompra.model';
import { RecepcionMercanciaService } from '../../../../core/services/costos/recepcionmercancia/recepcionmercancia.service';
import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { SelectorUsuarios } from '../../../../shared/reutilizable/selector-usuarios/selector-usuarios';
import { PermisosService } from '../../../../core/services/permisos.service';

@Component({
  selector: 'app-iniciar-repmercancia',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './iniciar-repmercancia.html',
  styleUrl: './iniciar-repmercancia.scss',
})
export class IniciarRepmercancia implements OnInit {

  orden!: OrdenCompra;
  form!: FormGroup;

  totalItems = 0;
  totalKg = 0;
  totalBultos = 0;

  guardando = false;

  recepcionGuardadaId: number | null = null;
  etiquetasError = false;

  constructor(
    private fb: FormBuilder,
    private toastr: ToastrService,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<IniciarRepmercancia>,
    private recepcionService: RecepcionMercanciaService,
    public permisos: PermisosService,
    @Inject(MAT_DIALOG_DATA) public data: OrdenCompra
  ) {
    this.orden = data;
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      conductor: ['', Validators.required],
      transportadora: ['', Validators.required],
      tipoDocumento: ['Factura', Validators.required],
      embalajeAdecuado: [true],
      recibe: ['', Validators.required],
      cargo: ['', Validators.required],
      observaciones: [''],
      detalles: this.fb.array([])
    });

    this.form.valueChanges.subscribe(() => this.calcularResumen());

    this.cargarFormulario();
  }

  // FORM ARRAYS

  get detalles(): FormArray {
    return this.form.get('detalles') as FormArray;
  }

  getLotes(item: AbstractControl): FormArray {
    return item.get('lotes') as FormArray;
  }

  // CREACIÓN DE LOTES

  private crearLote(
    cantidadRecibida: number = 0,
    bultosRecibidos: number = 0
  ): FormGroup {

    return this.fb.group({
      loteProveedor: ['', Validators.required],

      cantidadRecibida: [
        cantidadRecibida,
        [
          Validators.required,
          Validators.min(0.01)
        ]
      ],

      bultosRecibidos: [
        bultosRecibidos,
        [
          Validators.required,
          Validators.min(1),
          Validators.pattern(/^[0-9]+$/)
        ]
      ],

      estadoMaterial: ['Conforme', Validators.required],

      observaciones: ['']
    });
  }

  agregarLote(indiceDetalle: number): void {
    const detalle = this.detalles.at(indiceDetalle) as FormGroup;

    if (!detalle.get('seleccionado')?.value) {
      return;
    }

    this.getLotes(detalle).push(this.crearLote());

    this.getLotes(detalle).updateValueAndValidity();

    this.calcularResumen();
  }

  eliminarLote(indiceDetalle: number, indiceLote: number): void {
    const detalle = this.detalles.at(indiceDetalle) as FormGroup;
    const lotes = this.getLotes(detalle);

    if (lotes.length <= 1) {
      return;
    }

    lotes.removeAt(indiceLote);

    lotes.updateValueAndValidity();

    this.calcularResumen();
  }

  // CARGAR FORMULARIO

  cargarFormulario(): void {

    this.recepcionService.obtenerFormulario(this.orden.id).subscribe({

      next: (data) => {

        this.form.patchValue({
          recibe: data.recibe ?? '',
          cargo: data.cargo ?? ''
        });

        data.items.forEach((x: any) => {

          const lotes = this.fb.array(
            [
              this.crearLote(
                Number(x.cantidadPendiente),
                Number(x.bultosPendientes)
              )
            ],
            {
              validators: [this.validarDistribucionLotes()]
            }
          );

          const detalle = this.fb.group({

            ordenCompraDetalleId: [x.ordenCompraDetalleId],

            material: [x.material],

            cantidadOrdenada: [x.cantidad],

            bultosOrdenados: [x.bultos],

            cantidadRecibidaAnterior: [x.cantidadRecibida],

            bultosRecibidosAnterior: [x.bultosRecibidos],

            cantidadPendiente: [x.cantidadPendiente],

            bultosPendientes: [x.bultosPendientes],

            seleccionado: [true],

            lotes: lotes

          });

          detalle.get('seleccionado')?.valueChanges.subscribe(seleccionado => {

            this.actualizarValidadoresDetalle(
              detalle,
              seleccionado === true
            );

          });

          this.detalles.push(detalle);

          // Inicializar validaciones.
          this.actualizarValidadoresDetalle(detalle, true);

        });

        this.calcularResumen();
      },

      error: (error) => {

        if (error.status === 409) {
          this.toastr.warning(error.error.mensaje, 'Recepción');
          this.dialogRef.close();
          return;
        }

        this.toastr.error(
          'No fue posible cargar los materiales',
          'Error'
        );

      }

    });
  }

  // VALIDACIONES

  private actualizarValidadoresDetalle(
    detalle: FormGroup,
    seleccionado: boolean
  ): void {

    const lotes = this.getLotes(detalle);

    lotes.controls.forEach((control) => {

      const lote = control as FormGroup;

      const cantidad = lote.get('cantidadRecibida');
      const bultos = lote.get('bultosRecibidos');
      const loteProveedor = lote.get('loteProveedor');
      const estado = lote.get('estadoMaterial');

      if (seleccionado) {

        cantidad?.setValidators([
          Validators.required,
          Validators.min(0.01)
        ]);

        bultos?.setValidators([
          Validators.required,
          Validators.min(1),
          Validators.pattern(/^[0-9]+$/)
        ]);

        loteProveedor?.setValidators([
          Validators.required
        ]);

        estado?.setValidators([
          Validators.required
        ]);

      } else {

        cantidad?.clearValidators();
        bultos?.clearValidators();
        loteProveedor?.clearValidators();
        estado?.clearValidators();

      }

      cantidad?.updateValueAndValidity({ emitEvent: false });
      bultos?.updateValueAndValidity({ emitEvent: false });
      loteProveedor?.updateValueAndValidity({ emitEvent: false });
      estado?.updateValueAndValidity({ emitEvent: false });

    });

    lotes.updateValueAndValidity({ emitEvent: false });

  }

  /**
   * Valida que:
   * - El material seleccionado tenga al menos un lote.
   * - La suma de kg no supere los kg pendientes.
   * - La suma de bultos no supere los bultos pendientes.
   *
   * No obliga a recibir todo el pendiente porque se permiten
   * recepciones parciales.
   */
  private validarDistribucionLotes(): ValidatorFn {

    return (control: AbstractControl): ValidationErrors | null => {

      const detalle = control.parent;

      // El FormArray todavía puede no tener un padre
      // durante su construcción.
      if (!detalle || detalle.get('seleccionado')?.value !== true) {
        return null;
      }

      const lotes = control as FormArray;

      if (lotes.length === 0) {
        return { sinLotes: true };
      }

      const totalKg = lotes.controls.reduce(
        (total, lote) =>
          total + Number(lote.get('cantidadRecibida')?.value || 0),
        0
      );

      const totalBultos = lotes.controls.reduce(
        (total, lote) =>
          total + Number(lote.get('bultosRecibidos')?.value || 0),
        0
      );

      const kgPendientes = Number(
        detalle.get('cantidadPendiente')?.value || 0
      );

      const bultosPendientes = Number(
        detalle.get('bultosPendientes')?.value || 0
      );

      const errores: ValidationErrors = {};

      if (totalKg > kgPendientes + 0.000001) {
        errores['excesoKg'] = {
          recibido: totalKg,
          pendiente: kgPendientes
        };
      }

      if (totalBultos > bultosPendientes + 0.000001) {
        errores['excesoBultos'] = {
          recibido: totalBultos,
          pendiente: bultosPendientes
        };
      }

      return Object.keys(errores).length ? errores : null;

    };
  }

  // TOTALES POR MATERIAL

  calcularTotalLotes(
    item: AbstractControl,
    campo: 'cantidadRecibida' | 'bultosRecibidos'
  ): number {

    const lotes = this.getLotes(item);

    return lotes.controls.reduce(
      (total, lote) =>
        total + Number(lote.get(campo)?.value || 0),
      0
    );

  }

  // RESUMEN GENERAL

  calcularResumen(): void {

    const itemsSeleccionados = this.detalles.controls.filter(
      item => item.get('seleccionado')?.value === true
    );

    this.totalItems = itemsSeleccionados.length;

    this.totalKg = itemsSeleccionados.reduce(
      (total, item) =>
        total + this.calcularTotalLotes(item, 'cantidadRecibida'),
      0
    );

    this.totalBultos = itemsSeleccionados.reduce(
      (total, item) =>
        total + this.calcularTotalLotes(item, 'bultosRecibidos'),
      0
    );

  }

  actualizarResumen(): void {
    this.calcularResumen();
  }

  // FINALIZAR RECEPCIÓN

  finalizarRecepcion(): void {

    if (this.guardando) {
      return;
    }

    const itemsSeleccionados = this.detalles.controls.filter(
      item => item.get('seleccionado')?.value === true
    );

    if (itemsSeleccionados.length === 0) {
      this.toastr.warning(
        'Seleccione al menos un material para recibir.',
        'Validación'
      );
      return;
    }

    this.form.updateValueAndValidity();

    if (this.form.invalid) {

      this.form.markAllAsTouched();

      this.toastr.warning(
        'Revise los campos obligatorios y las cantidades distribuidas entre los lotes.',
        'Validación'
      );

      return;
    }

    const datos = this.form.getRawValue();

    /*
     * IMPORTANTE:
     * Se convierte cada lote en un detalle independiente.
     *
     * Seconservamos el formato que actualmente espera
     * el backend: un registro por lote.
     */
    const detallesRecepcion = datos.detalles
      .filter((item: any) => item.seleccionado)
      .flatMap((item: any) =>
        item.lotes.map((lote: any) => ({

          ordenCompraDetalleId: Number(item.ordenCompraDetalleId),

          cantidadRecibida: Number(lote.cantidadRecibida),

          bultosRecibidos: Number(lote.bultosRecibidos),

          loteProveedor: lote.loteProveedor.trim(),

          estadoMaterial: lote.estadoMaterial,

          observaciones: lote.observaciones || null

        }))
      );

    const recepcion: any = {

      ordenCompraId: this.orden.id,

      conductor: datos.conductor,

      transportadora: datos.transportadora,

      tipoDocumento: datos.tipoDocumento,

      embalajeAdecuado: datos.embalajeAdecuado,

      recibe: datos.recibe,

      cargo: datos.cargo,

      observaciones: datos.observaciones,

      detalles: detallesRecepcion

    };

    this.dialog.open(SelectorUsuarios, {
      width: '90vw',
      maxWidth: '1400px',
      height: '85vh',
      disableClose: true
    }).afterClosed().subscribe(idsUsuarios => {

      if (!idsUsuarios?.length) {

        this.toastr.warning(
          'Seleccione al menos un destinatario.',
          'Correo'
        );

        return;
      }

      this.guardando = true;

      recepcion.usuarios = idsUsuarios;

      this.recepcionService.crear(recepcion).subscribe({

        next: respuesta => {

          this.recepcionService.obtenerEtiquetas(respuesta.id).subscribe({

            next: pdf => {

              const url = URL.createObjectURL(pdf);

              window.open(url, '_blank');

              this.toastr.success(
                `${respuesta.totalBultos} bultos recibidos correctamente.`,
                'Recepción Finalizada'
              );

              this.dialogRef.close(respuesta);

            },

            error: () => {

              this.guardando = false;

              this.recepcionGuardadaId = respuesta.id;
              this.etiquetasError = true;

              this.toastr.warning(
                'La recepción fue guardada correctamente, pero no fue posible generar las etiquetas.',
                'Recepción'
              );

            }

          });

        },

        error: error => {

          this.guardando = false;

          if (error.status === 409) {

            this.toastr.warning(
              error.error?.mensaje ?? 'La solicitud ya está siendo procesada.',
              'Recepción'
            );

            return;
          }

          this.toastr.error(
            'No fue posible guardar la recepción',
            'Error'
          );

        }

      });

    });

  }

  cerrar(): void {

    if (this.guardando) {
      return;
    }

    this.dialogRef.close();

  }

}