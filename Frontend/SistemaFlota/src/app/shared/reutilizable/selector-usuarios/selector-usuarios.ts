import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { UsuariosService } from '../../../core/services/usuarios.service';
import { MatDialogRef } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-selector-usuarios',
  standalone: true,
  imports: [
    FormsModule,
    CommonModule,
  ],
  templateUrl: './selector-usuarios.html',
  styleUrl: './selector-usuarios.scss',
})
export class SelectorUsuarios implements OnInit {

  usuarios: any[] = [];
  filtro = '';

  @Output()
  seleccionChange = new EventEmitter<number[]>();

  constructor(
    private usuarioService: UsuariosService,
    private toastr: ToastrService,
    private dialogRef: MatDialogRef<SelectorUsuarios>
  ) { }

  ngOnInit(): void {
    this.cargarUsuarios();
  }

  cargarUsuarios(): void {

    this.usuarioService.obtenerDestinatarios().subscribe({
      next: resp => {
        this.usuarios = resp.map(x => ({
          ...x,
          seleccionado: false
        }));
      },
      error: () => {
        this.toastr.error(
          'No fue posible cargar los usuarios.',
          'Error'
        );
      }
    });

  }

  get usuariosFiltrados() {

    return this.usuarios.filter(x =>

      x.username
        .toLowerCase()
        .includes(this.filtro.toLowerCase())
      ||
      (x.email ?? '')
        .toLowerCase()
        .includes(this.filtro.toLowerCase())
    );
  }

  cambiarSeleccion(): void {
    const ids = this.usuarios
      .filter(x => x.seleccionado)
      .map(x => x.id);
    this.seleccionChange.emit(ids);
  }

  enviar(): void {

    const ids = this.usuarios
      .filter(x => x.seleccionado)
      .map(x => x.id);

    if (ids.length === 0) {

      this.toastr.warning(
        'Seleccione al menos un destinatario',
        'Correo'
      );

      return;
    }
    this.dialogRef.close(ids);
  }

  cerrar() {
    this.dialogRef.close(true);
  }
}
