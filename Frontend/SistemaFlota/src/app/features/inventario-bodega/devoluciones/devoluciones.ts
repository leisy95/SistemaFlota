import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

interface Cliente {
  id: number;
  nombre: string;
}

interface InventarioItem {
  id: number;
  referencia: string;
  referenciaPeso: string;
  pesoActual: number;
}

@Component({
  selector: 'app-devoluciones',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './devoluciones.html',
  styleUrl: './devoluciones.scss',
})
export class Devoluciones {

  form: FormGroup;

  cargando = false;
  inventarioItemId: number | null = null;

  dataSource: InventarioItem[] = [];

  // CLIENTES QUEMADOS
  clientes: Cliente[] = [
    { id: 1, nombre: 'Plásticos Industriales S.A.S.' },
    { id: 2, nombre: 'Empaques del Valle S.A.S.' },
    { id: 3, nombre: 'Distribuciones Nacionales' },
    { id: 4, nombre: 'Comercializadora Andina' },
    { id: 5, nombre: 'Industrias del Pacífico' },
    { id: 6, nombre: 'Empaques Flexibles S.A.S.' },
    { id: 7, nombre: 'Industrias Plásticas de Colombia' },
    { id: 8, nombre: 'Comercializadora Industrial' }
  ];

  // INVENTARIO QUEMADO
  inventario: InventarioItem[] = [
    {
      id: 1,
      referencia: 'MAT-001',
      referenciaPeso: 'REF-P001',
      pesoActual: 25
    },
    {
      id: 2,
      referencia: 'MAT-002',
      referenciaPeso: 'REF-P002',
      pesoActual: 30
    },
    {
      id: 3,
      referencia: 'MAT-003',
      referenciaPeso: 'REF-P003',
      pesoActual: 45
    },
    {
      id: 4,
      referencia: 'MAT-004',
      referenciaPeso: 'REF-P004',
      pesoActual: 20
    },
    {
      id: 5,
      referencia: 'MAT-005',
      referenciaPeso: 'REF-P005',
      pesoActual: 50
    },
    {
      id: 6,
      referencia: 'MAT-006',
      referenciaPeso: 'REF-P006',
      pesoActual: 35
    },
    {
      id: 7,
      referencia: 'MAT-007',
      referenciaPeso: 'REF-P007',
      pesoActual: 40
    },
    {
      id: 8,
      referencia: 'MAT-008',
      referenciaPeso: 'REF-P008',
      pesoActual: 15
    }
  ];

  constructor(
    private fb: FormBuilder,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.form = this.fb.group({
      referenciaPeso: ['', Validators.required],
      clienteId: ['', Validators.required]
    });
  }

  // BUSCAR PRODUCTO POR CÓDIGO
  buscarItem(): void {

    const codigo = this.form.get('referenciaPeso')?.value?.trim();

    if (!codigo) {
      this.form.get('referenciaPeso')?.markAsTouched();
      this.toastr.warning('Ingrese o escanee un código de barras.');
      return;
    }

    const item = this.inventario.find(
      x => x.referenciaPeso.toLowerCase() === codigo.toLowerCase()
    );

    if (!item) {
      this.inventarioItemId = null;
      this.dataSource = [];

      this.toastr.error('No se encontró ningún producto con ese código.');
      return;
    }

    this.inventarioItemId = item.id;
    this.dataSource = [item];

    this.toastr.success('Producto encontrado correctamente.');
  }

  // REGISTRAR DEVOLUCIÓN
  confirmarDevolucion(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toastr.warning('Complete todos los campos obligatorios.');
      return;
    }

    if (!this.inventarioItemId || this.dataSource.length === 0) {
      this.toastr.warning('Primero debe buscar un producto válido.');
      return;
    }

    this.cargando = true;

    // Simulación de registro
    setTimeout(() => {

      const cliente = this.clientes.find(
        x => x.id === Number(this.form.value.clienteId)
      );

      this.toastr.success(
        `Devolución registrada correctamente para ${cliente?.nombre}.`
      );

      this.limpiarFormulario();
      this.cargando = false;

    }, 700);
  }

  // REGRESAR
  regresar(): void {
    this.limpiarFormulario();
    this.router.navigate(['/inventario-bodega/mostrar-entradas']);
  }

  // LIMPIAR FORMULARIO
  private limpiarFormulario(): void {
    this.form.reset({
      referenciaPeso: '',
      clienteId: ''
    });

    this.dataSource = [];
    this.inventarioItemId = null;
    this.cargando = false;
  }

}