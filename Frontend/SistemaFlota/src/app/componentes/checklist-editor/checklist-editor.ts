import {
  Component,
  OnInit
}
from '@angular/core';

import { CommonModule }
from '@angular/common';

import { FormsModule }
from '@angular/forms';

import { ChecklistService }
from '../../services/checklist.service';

import { TiposVehiculoService }
from '../../services/tipos-vehiculo.service';

@Component({
  selector: 'app-checklist-editor',

  standalone: true,

  imports: [
    CommonModule,
    FormsModule
  ],

  templateUrl:
    './checklist-editor.html',

  styleUrls: [
    './checklist-editor.scss'
  ]
})

export class ChecklistEditorComponent
implements OnInit {

  checklist: any[] = [];

  paginaActual = 1;

itemsPorPagina = 5;

  tiposVehiculo: any[] = [];

  todosChecklist: any[] = [];

  descripcion = '';

  tipoVehiculoId = 0;

  editando = false;

  editandoId = 0;

  constructor(

    private checklistService:
      ChecklistService,

    private tiposVehiculoService:
      TiposVehiculoService

  ) {}

  ngOnInit(): void {

    this.obtenerTipos();

    this.obtenerTodos();

  }

  // OBTENER TIPOS

  obtenerTipos() {

    this.tiposVehiculoService
      .obtenerTipos()
      .subscribe({

        next: (data) => {

          this.tiposVehiculo =
            data;

        },

        error: (err) => {

          console.error(err);

        }

      });

  }

  // OBTENER TODOS

  obtenerTodos() {

    this.checklistService
      .obtenerTodos()
      .subscribe({

        next: (data) => {

          this.todosChecklist =
            data;

        },

        error: (err) => {

          console.error(err);

        }

      });

  }

  // GUARDAR / EDITAR

  guardarChecklist() {

    if (
      this.tipoVehiculoId === 0
    ) {

      alert(
        'Seleccione tipo vehículo'
      );

      return;

    }

    if (
      this.descripcion.trim()
      === ''
    ) {

      alert(
        'Ingrese descripción'
      );

      return;

    }

    const data = {

      descripcion:
        this.descripcion,

      tipoVehiculoId:
        this.tipoVehiculoId

    };

    // EDITAR

    if (this.editando) {

      this.checklistService
        .editarChecklist(
          this.editandoId,
          data
        )
        .subscribe({

          next: () => {

            alert(
              'Checklist actualizado'
            );

            this.limpiarFormulario();

            this.obtenerTodos();

          },

          error: (err) => {

            console.error(err);

            alert(
              'Error al actualizar'
            );

          }

        });

      return;

    }

    // CREAR

    this.checklistService
      .crearChecklist(data)
      .subscribe({

        next: () => {

          alert(
            'Checklist guardado'
          );

          this.limpiarFormulario();

          this.obtenerTodos();

        },

        error: (err) => {

          console.error(err);

          alert(
            'Error al guardar'
          );

        }

      });

  }

  // EDITAR

  editarChecklist(
    item: any
  ) {

    this.editando = true;

    this.editandoId =
      item.id;

    this.descripcion =
      item.descripcion;

    this.tipoVehiculoId =
      item.tipoVehiculoId;

  }

  // ELIMINAR

  eliminarChecklist(
    id: number
  ) {

    const confirmar =

      confirm(
        '¿Eliminar checklist?'
      );

    if (!confirmar) {

      return;

    }

    this.checklistService
      .eliminarChecklist(id)
      .subscribe({

        next: () => {

          alert(
            'Checklist eliminado'
          );

          this.obtenerTodos();

        },

        error: (err) => {

          console.error(err);

          alert(
            'Error al eliminar'
          );

        }

      });

  }

  // LIMPIAR

  limpiarFormulario() {

    this.descripcion = '';

    this.tipoVehiculoId = 0;

    this.editando = false;

    this.editandoId = 0;

  }

  obtenerChecklistPaginado() {

  const inicio =

    (this.paginaActual - 1)
    * this.itemsPorPagina;

  const fin =

    inicio + this.itemsPorPagina;

  return this.todosChecklist
    .slice(inicio, fin);

}

paginaSiguiente() {

  const totalPaginas =

    Math.ceil(

      this.todosChecklist.length
      / this.itemsPorPagina

    );

  if (
    this.paginaActual
    < totalPaginas
  ) {

    this.paginaActual++;

  }

}

paginaAnterior() {

  if (
    this.paginaActual > 1
  ) {

    this.paginaActual--;

  }

}

}