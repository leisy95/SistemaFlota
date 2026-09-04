import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { OrdenCompra } from '../../../../core/models/costos/ordenCompra/ordencompra.model';
import { RecepcionMercanciaService } from '../../../../core/services/costos/recepcionmercancia/recepcionmercancia.service';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
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

  get detalles(): FormArray {
    return this.form.get('detalles') as FormArray;
  }

  cargarFormulario() {
    this.recepcionService.obtenerFormulario(this.orden.id).subscribe({
      next: (data) => {
        this.form.patchValue({ recibe: data.recibe ?? '', cargo: data.cargo ?? '' });
        data.items.forEach((x: any) => {
          this.detalles.push(this.fb.group({
            ordenCompraDetalleId: [x.ordenCompraDetalleId],
            material: [x.material],
            cantidadOrdenada: [x.cantidad],
            bultosOrdenados: [x.bultos],
            cantidadRecibidaAnterior: [x.cantidadRecibida],
            bultosRecibidosAnterior: [x.bultosRecibidos],
            cantidadPendiente: [x.cantidadPendiente],
            bultosPendientes: [x.bultosPendientes],
            seleccionado: [true],
            cantidadRecibida: [x.cantidadPendiente, [Validators.required, Validators.min(0.01), Validators.max(x.cantidadPendiente)]],
            bultosRecibidos: [Number(x.bultosPendientes), [Validators.required, Validators.min(0.01), Validators.max(x.bultosPendientes)]],
            loteProveedor: ['', Validators.required],
            estadoMaterial: ['Conforme', Validators.required],
            observaciones: ['']
          }));
        });
        this.calcularResumen();
      },
      error: (error) => {
        if (error.status === 409) {
          this.toastr.warning(error.error.mensaje, 'Recepción');
          this.dialogRef.close();
          return;
        }
        this.toastr.error('No fue posible cargar los materiales');
      }
    });
  }

  calcularResumen(): void {
    const items = this.detalles.controls.filter(x => x.get('seleccionado')?.value);
    this.totalItems = items.length;
    this.totalKg = items.reduce((a, b) => a + Number(b.get('cantidadRecibida')?.value || 0), 0);
    this.totalBultos = items.reduce((a, b) => a + Number(b.get('bultosRecibidos')?.value || 0), 0);
  }

  actualizarResumen(): void {
    this.calcularResumen();
  }

  finalizarRecepcion(): void {
    if (this.guardando) return;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastr.warning('Complete los campos obligatorios.', 'Validación');
      return;
    }

    const datos = this.form.getRawValue();
    const recepcion = {
      ordenCompraId: this.orden.id,
      ...datos,
      detalles: datos.detalles.filter((x: any) => x.seleccionado).map((x: any) => ({
        ordenCompraDetalleId: Number(x.ordenCompraDetalleId),
        cantidadRecibida: Number(x.cantidadRecibida),
        bultosRecibidos: Number(x.bultosRecibidos),
        loteProveedor: x.loteProveedor,
        estadoMaterial: x.estadoMaterial,
        observaciones: x.observaciones || null
      }))
    };

    this.dialog.open(SelectorUsuarios, {
      width: '90vw',
      maxWidth: '1400px',
      height: '85vh',
      disableClose: true
    }).afterClosed().subscribe(idsUsuarios => {
      if (!idsUsuarios?.length) {
        this.toastr.warning('Seleccione al menos un destinatario.', 'Correo');
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
              this.toastr.success(`${respuesta.totalBultos} bultos recibidos correctamente.`, 'Recepción Finalizada');
              this.dialogRef.close(respuesta);
            },
            error: () => {
              this.guardando = false;
              this.toastr.warning('La recepción fue guardada, pero no fue posible generar las etiquetas.', 'Recepción');
            }
          });
        },
        error: error => {
          this.guardando = false;

          if (error.status === 409) {
            this.toastr.warning(error.error?.mensaje ?? 'La solicitud ya está siendo procesada.', 'Recepción');
            return;
          }

          this.toastr.error('No fue posible guardar la recepción', 'Error');
        }
      });
    });
  }

  cerrar(): void {
    if (this.guardando) return;
    this.dialogRef.close();
  }
}